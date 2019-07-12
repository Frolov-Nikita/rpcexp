using ModbusBasic;
using RPCExp.Common;
using RPCExp.Modbus.TypeConverters;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RPCExp.Modbus
{
    public class Device : DeviceAbstract
    {
        ModbusFactory factory = new ModbusFactory(); //TODO ПЕРЕДЕЛАТЬ!!

        private Dictionary<ModbusValueType, TypeConverterAbstract> typeConverters = new Dictionary<ModbusValueType, TypeConverterAbstract>();

        private void UpdateTypeConverters() {
            typeConverters.Clear();
            typeConverters.Add(ModbusValueType.Float, new TypeConverterFloat(ByteOrder));
            typeConverters.Add(ModbusValueType.Int16, new TypeConverterInt16(ByteOrder));
            typeConverters.Add(ModbusValueType.Int32, new TypeConverterInt32(ByteOrder));
        }

        private TypeConverterAbstract GetTypeConverter(ModbusValueType modbusValueType)
        {
            if (typeConverters.ContainsKey(modbusValueType))
                return typeConverters[modbusValueType];
            UpdateTypeConverters();
            return typeConverters[modbusValueType];
        }

        private byte[] byteOrder = new byte[] { 2, 3, 0, 1 };

        public byte SlaveId { get; set; }
        
        public byte[] ByteOrder {
            get => byteOrder;
            set {
                byteOrder = value;
                UpdateTypeConverters();
            }
        }

        public ConnectionSource Connection { get; set; }

        public MasterSource MasterSource { get; set; }
                
        protected override async Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            long nextTime = 0, waitTime = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (Connection.IsOpen)
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

        private async Task UpdateHoldingRegisters(IModbusMaster master, MTagsGroup g)
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

        private async Task UpdateInputRegisters(IModbusMaster master, MTagsGroup g)
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

        private async Task UpdateCoils(IModbusMaster master, MTagsGroup g)
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

        private async Task UpdateDiscreteInputs(IModbusMaster master, MTagsGroup g)
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
            var tags = NeedToUpdate(out long groupNextUpdate, force);

            var holdingRegisters = new MTagsGroup();
            var inputRegisters = new MTagsGroup();
            var coils = new MTagsGroup();
            var discreteInputs = new MTagsGroup();

            foreach (MTag tag in tags)
            {
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

            if (tags.Count > 0)
            {
                IModbusMaster master = MasterSource.Get(factory,Connection);

                foreach (var g in (coils).Slice())
                    await UpdateCoils(master, g);

                foreach (var g in (discreteInputs).Slice())
                    await UpdateDiscreteInputs(master, g);

                foreach (var g in (inputRegisters).Slice())
                    await UpdateInputRegisters(master, g);

                foreach (var g in (holdingRegisters).Slice())
                    await UpdateHoldingRegisters(master, g);
            }

            return groupNextUpdate;
        }

        /// <summary>
        /// Записать значения тегов в устройство 
        /// (Еще не реализовано)
        /// </summary>
        /// <param name="tagsValues"></param>
        /// <returns></returns>
        public override async Task<int> Write(IDictionary<string, object> tagsValues)
        {
            
            List<MTag> tags = new List<MTag>();
            foreach (var tv in tagsValues)
                if (Tags.ContainsKey(tv.Key))
                    tags.Add((MTag)Tags[tv.Key]);

            if(tags.Count > 0)
            {
                var holdingRegisters = new MTagsGroup();
                var coils = new MTagsGroup();

                foreach (MTag tag in tags)
                {
                    switch (tag.Region)
                    {
                        case ModbusRegion.Coils:
                            coils.Add(tag);
                            break;
                        case ModbusRegion.HoldingRegisters:
                            holdingRegisters.Add(tag);
                            break;
                        default:
                            break;
                    }
                }

                IModbusMaster master = MasterSource.Get(factory, Connection);

                foreach (var g in holdingRegisters.Slice())
                {
                    ushort[] values = new ushort[g.Length];
                    byte[] buff = new byte[32];

                    foreach (var t in g)
                    {
                        var v = tagsValues[t.Name];
                        var tc = GetTypeConverter(t.ValueType);
                        
                        tc.GetBytes(buff, v);
                        var b = t.Begin - g.Begin;
                        for (int i = 0; i < t.Length; i++)
                            values[b + i] = BitConverter.ToUInt16(buff, i * 2);
                    }

                    await master.WriteMultipleRegistersAsync(SlaveId, (ushort)g.Begin, values);
                }

                foreach (var g in coils.Slice())
                {
                    var values = new bool[g.Length];
                    int i = 0;
                    foreach (var t in g)
                        values[i++] = (bool)tagsValues[t.Name];
                    await master.WriteMultipleCoilsAsync(SlaveId, (ushort)g.Begin, values);
                }
            }

            return 0;
            //throw new NotImplementedException();
        }
    }
}
