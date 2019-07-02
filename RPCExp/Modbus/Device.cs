using NModbus;
using RPCExp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPCExp.Modbus
{

    public class Device: DeviceAbstract
    {        
        public byte SlaveId { get; set; }

        public string Host { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 11502;
        
        public long BadCommWaitPeriod { get; set; } = 10 * 10_000_000;

        private System.Net.Sockets.TcpClient client;

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
                    nextTime = await Update( nextTime == 0);

                    waitTime = nextTime - DateTime.Now.Ticks;
                    waitTime = waitTime > 10_000 ? waitTime : 10_000; // 10_000 = 1 миллисекунда
                    waitTime = waitTime > 50_000_000 ? waitTime / 2 : waitTime;
                    waitTime = waitTime < 50_000_000 ? waitTime : 50_000_000;// 100_000_000 = 10 сек

                    await Task.Delay(TimeSpan.FromTicks(waitTime));
                }
                else
                {
                    foreach (var t in Tags)
                        t.Value.SetValue(null, TagQuality.BAD_COMM_FAILURE);
                    await Task.Delay(TimeSpan.FromTicks(BadCommWaitPeriod));
                }
            }
        }

        private async Task UpdateHoldingRegisters(MTagsGroup g)
        {
            try { 
                var registers = await master.ReadHoldingRegistersAsync(SlaveId, (ushort)g.Begin, (ushort)g.Length).ConfigureAwait(false);

                var buff = new byte[g.Length * 2];

                for (var k = 0; k < g.Length; k++)
                    BitConverter.GetBytes(registers[k])
                        .CopyTo(buff, (k) * 2);

                foreach (var t in g)
                    t.SetValue(buff.AsSpan((t.Begin - g.Begin)*2));
            }
            catch
            {
                foreach (var t in g)
                    t.SetValue(null, TagQuality.BAD);
            }
}

        private async Task UpdateInputRegisters(MTagsGroup g)
        {
            try { 
                var registers = await master.ReadInputRegistersAsync(SlaveId, (ushort)g.Begin, (ushort)g.Length).ConfigureAwait(false);

                var buff = new byte[g.Length * 2];

                for (var k = 0; k < g.Length; k++)
                    BitConverter.GetBytes(registers[k])
                        .CopyTo(buff, (k) * 2);

                foreach (var t in g)
                    t.SetValue(buff.AsSpan((t.Begin - g.Begin) * 2));

            }
            catch
            {
                foreach (var t in g)
                    t.SetValue(null, TagQuality.BAD);
            }
}

        private async Task UpdateCoils(MTagsGroup g)
        {
            try { 
                var bits = await master.ReadCoilsAsync(SlaveId, (ushort)g.Begin, (ushort)g.Length).ConfigureAwait(false);

                foreach (var t in g)
                    t.SetValue( bits[t.Begin - g.Begin]);
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
            var afterTick = DateTime.Now.Ticks + TimeSpan.FromSeconds(1).Ticks;
            long groupNextUpdate = long.MaxValue;

            var holdingRegisters = new MTagsGroup();
            var inputRegisters = new MTagsGroup();
            var coils = new MTagsGroup();
            var discreteInputs = new MTagsGroup();

            var count = 0;

            foreach (var tvp in Tags)
            {
                var tag = (MTag)tvp.Value;
                if ((!tag.StatIsAlive)&&(!force))
                    continue;

                long tagNextTick = (tag.Quality == TagQuality.GOOD) ?
                    tag.Last + tag.StatPeriod:
                    tag.Last + BadCommWaitPeriod;

                if (tagNextTick > afterTick)
                {
                    if (groupNextUpdate > tagNextTick)
                        groupNextUpdate = tagNextTick;

                    if (!force)
                        continue;
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

            if(count > 0)
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
            throw new NotImplementedException();
        }
    }
}
