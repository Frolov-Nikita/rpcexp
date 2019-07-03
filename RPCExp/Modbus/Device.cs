using NModbus;
using RPCExp.Common;
using RPCExp.Modbus.TypeConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPCExp.Modbus
{

    public class Device : DeviceAbstract
    {

        private Dictionary<ModbusValueType, TypeConverterAbstract> typeConverters = new Dictionary<ModbusValueType, TypeConverterAbstract>();

        private void updateTypeConverters() {
            typeConverters.Clear();
            typeConverters.Add(ModbusValueType.Bool, new TypeConverterBool(ByteOrder));
            typeConverters.Add(ModbusValueType.Float, new TypeConverterFloat(ByteOrder));
            typeConverters.Add(ModbusValueType.Int16, new TypeConverterInt16(ByteOrder));
            typeConverters.Add(ModbusValueType.Int32, new TypeConverterInt32(ByteOrder));
        }

        private TypeConverterAbstract GetTypeConverter(ModbusValueType modbusValueType)
        {
            if (typeConverters.ContainsKey(modbusValueType))
                return typeConverters[modbusValueType];
            updateTypeConverters();
            return typeConverters[modbusValueType];
        }

        private byte[] byteOrder = new byte[] { 2, 3, 0, 1 };

        public byte SlaveId { get; set; }

        public string Host { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 11502;

        private System.Net.Sockets.TcpClient client;

        public long BadCommWaitPeriod { get; set; } = 10 * 10_000_000;

        public bool InActiveUpdate { get; set; } = true;

        public long InActiveUpdatePeriod { get; set; } = 20 * 10_000_000;

        public byte[] ByteOrder {
            get => byteOrder;
            set {
                byteOrder = value;
                updateTypeConverters();
            }
        } 

        private IModbusMaster master;

        private bool ConnectedOrConnect()
        {
            if ((client != null) && client.Connected)
                return true;

            try
            {
                var factory = new ModbusFactory();
                client = new System.Net.Sockets.TcpClient(Host, Port);
                master = factory.CreateMaster(client);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        protected override async Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            long nextTime = 0, waitTime = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (ConnectedOrConnect())
                {
                    nextTime = await Update(nextTime == 0);

                    waitTime = nextTime - DateTime.Now.Ticks;
                    waitTime = waitTime > 10_000 ? waitTime : 10_000; // 10_000 = 1 миллисекунда
                    waitTime = waitTime > 50_000_000 ? waitTime / 2 : waitTime;
                    waitTime = waitTime < 50_000_000 ? waitTime : 50_000_000;// 100_000_000 = 10 сек

                    await Task.Delay((int)waitTime / 10_000);
                }
                else
                {
                    foreach (var t in Tags)
                        t.Value.SetValue(null, TagQuality.BAD_COMM_FAILURE);
                    await Task.Delay((int)BadCommWaitPeriod / 10_000);
                }
            }
        }

        private async Task UpdateHoldingRegisters(MTagsGroup g)
        {
            try
            {
                var registers = await master.ReadHoldingRegistersAsync(SlaveId, (ushort)g.Begin, (ushort)g.Length).ConfigureAwait(false);

                var buff = new byte[g.Length * 2];

                for (var k = 0; k < g.Length; k++)
                    BitConverter.GetBytes(registers[k])
                        .CopyTo(buff, (k) * 2);

                foreach (var t in g)
                {
                    var tc = GetTypeConverter(t.ValueType);
                    var val = tc.GetValue(buff.AsSpan((t.Begin - g.Begin) * 2));
                    t.SetValue(val);
                }
            }
            catch
            {
                foreach (var t in g)
                    t.SetValue(null, TagQuality.BAD);
            }
        }

        private async Task UpdateInputRegisters(MTagsGroup g)
        {
            try
            {
                var registers = await master.ReadInputRegistersAsync(SlaveId, (ushort)g.Begin, (ushort)g.Length).ConfigureAwait(false);

                var buff = new byte[g.Length * 2];

                for (var k = 0; k < g.Length; k++)
                    BitConverter.GetBytes(registers[k])
                        .CopyTo(buff, (k) * 2);

                foreach (var t in g)
                {
                    var tc = GetTypeConverter(t.ValueType);
                    var val = tc.GetValue(buff.AsSpan((t.Begin - g.Begin) * 2));
                    t.SetValue(val);
                }
            }
            catch
            {
                foreach (var t in g)
                    t.SetValue(null, TagQuality.BAD);
            }
        }

        private async Task UpdateCoils(MTagsGroup g)
        {
            try
            {
                var bits = await master.ReadCoilsAsync(SlaveId, (ushort)g.Begin, (ushort)g.Length).ConfigureAwait(false);

                foreach (var t in g)
                    t.SetValue(bits[t.Begin - g.Begin]);
            }
            catch
            {
                foreach (var t in g)
                    t.SetValue(null, TagQuality.BAD);
            }
        }

        private async Task UpdateDiscreteInputs(MTagsGroup g)
        {
            try
            {
                var bits = await master.ReadCoilsAsync(SlaveId, (ushort)g.Begin, (ushort)g.Length).ConfigureAwait(false);
                foreach (var t in g)
                    t.SetValue(bits[t.Begin - g.Begin]);
            }
            catch
            {
                foreach (var t in g)
                    t.SetValue(null, TagQuality.BAD);
            }
        }

        private async Task<long> Update(bool force = false)
        {
            var nowTick = DateTime.Now.Ticks;
            var afterTick = nowTick + TimeSpan.FromSeconds(1).Ticks;
            long groupNextUpdate = nowTick + BadCommWaitPeriod;

            var holdingRegisters = new MTagsGroup();
            var inputRegisters = new MTagsGroup();
            var coils = new MTagsGroup();
            var discreteInputs = new MTagsGroup();

            var count = 0;

            foreach (MTag tag in Tags.Values)
            {
                if ((!tag.StatIsAlive) && (!InActiveUpdate) && (!force))
                    continue;

                long period = (tag.Quality == TagQuality.GOOD) ?
                    tag.StatPeriod :
                    BadCommWaitPeriod;

                if ((!tag.StatIsAlive) && InActiveUpdate)
                    period = InActiveUpdatePeriod;

                long tagNextTick = tag.Last + period;

                if (tagNextTick > afterTick)
                {
                    if (groupNextUpdate > tagNextTick)
                        groupNextUpdate = tagNextTick;
                    if (!force)
                        continue;
                }
                else
                {
                    tagNextTick = nowTick + period;
                    if (groupNextUpdate > tagNextTick)
                        groupNextUpdate = tagNextTick;
                }

                count++;

                switch (tag.Region)
                {
                    case ModbusRegion.Coils:
                        coils.Add(tag);
                        break;
                    case ModbusRegion.DiscreteInputs:
                        discreteInputs.Add(tag);
                        break;
                    case ModbusRegion.InputRegisters:
                        inputRegisters.Add(tag);
                        break;
                    case ModbusRegion.HoldingRegisters:
                        holdingRegisters.Add(tag);
                        break;
                    default:
                        break;
                }
            }

            if (count > 0)
            {
                foreach (var g in (coils).Slice())
                    await UpdateCoils(g);

                foreach (var g in (discreteInputs).Slice())
                    await UpdateDiscreteInputs(g);

                foreach (var g in (inputRegisters).Slice())
                    await UpdateInputRegisters(g);

                foreach (var g in (holdingRegisters).Slice())
                    await UpdateHoldingRegisters(g);
            }

            return groupNextUpdate;
        }

        public override async Task<bool> Write(IDictionary<string, object> tagsValues)
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        }
    }
}
