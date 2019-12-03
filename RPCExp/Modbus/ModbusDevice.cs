using ModbusBasic;
using RPCExp.Common;
using RPCExp.Connections;
using RPCExp.Modbus.TypeConverters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RPCExp.Modbus
{

    public enum FrameType
    {
        Ip,
        Rtu,
        Ascii,
    }
    
    public class ModbusDevice : DeviceAbstract
    {
        static readonly ModbusFactory factory = new ModbusFactory();
        
        private static readonly Dictionary<Common.ValueType, TypeConverterAbstract> typeConverters = new Dictionary<Common.ValueType, TypeConverterAbstract> ();

        public int MaxGroupLength { get; set; } = 100;

        public int MaxGroupSpareLength { get; set; } = 0;

        private void UpdateTypeConverters() {
            typeConverters.Clear();
            typeConverters.Add(Common.ValueType.Float, new TypeConverterFloat(ByteOrder));
            typeConverters.Add(Common.ValueType.Int16, new TypeConverterInt16(ByteOrder));
            typeConverters.Add(Common.ValueType.Int32, new TypeConverterInt32(ByteOrder));
        }

        private TypeConverterAbstract GetTypeConverter(Common.ValueType modbusValueType)
        {
            if (typeConverters.ContainsKey(modbusValueType))
                return typeConverters[modbusValueType];
            UpdateTypeConverters();
            return typeConverters[modbusValueType];
        }

        private byte[] byteOrder = new byte[] {  0, 1, 2, 3 };

        public byte SlaveId { get; set; }

#pragma warning disable CA1819 // Свойства не должны возвращать массивы
        public byte[] ByteOrder {
            get => byteOrder;
            set {
                byteOrder = value;
                UpdateTypeConverters();
            }
        }        
#pragma warning restore CA1819 // Свойства не должны возвращать массивы

        public FrameType FrameType { get; set; } = FrameType.Ip;

        private readonly MasterSource masterSource = new MasterSource();

        private async Task UpdateHoldingRegisters(IModbusMaster master, MTagsCollection g)
        {
            await master.ReadHoldingRegistersAsync(SlaveId, (ushort)g.Begin, (ushort)g.Length)
                .ContinueWith(
                (TResult) =>
                {
                    if (TResult.Status == TaskStatus.RanToCompletion)
                    { // успех
                        var data = TResult.Result;

                        var buff = new byte[g.Length * 2];

                        for (var k = 0; k < g.Length; k++)
                            BitConverter.GetBytes(data[k])
                                .CopyTo(buff, (k) * 2);

                        foreach (var t in g)
                        {
                            var tc = GetTypeConverter(t.ValueType);
                            var val = tc.GetValue(buff.AsSpan((t.Begin - g.Begin) * 2));

                            t.SetValue(val);
                        }
                    }
                    else
                    { // сбой
                        foreach (var t in g)
                            t.SetValue(null, TagQuality.BAD);
                    }
                }).ConfigureAwait(false);
        }

        private async Task UpdateInputRegisters(IModbusMaster master, MTagsCollection g)
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

        private async Task UpdateCoils(IModbusMaster master, MTagsCollection g)
        {
            await master.ReadCoilsAsync(SlaveId, (ushort)g.Begin, (ushort)g.Length)
                .ContinueWith(
                (TResult) => 
                {
                    if(TResult.Status == TaskStatus.RanToCompletion)
                    { // успех
                        var data = TResult.Result;
                        foreach (var t in g)
                            t.SetValue(data[t.Begin - g.Begin]);
                    }
                    else
                    { // сбой
                        foreach (var t in g)
                            t.SetValue(null, TagQuality.BAD);
                    }
                }).ConfigureAwait(false);
        }

        private async Task UpdateDiscreteInputs(IModbusMaster master, MTagsCollection g)
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

        
        protected override async Task Read(ICollection<TagAbstract> tags, CancellationToken cancellationToken)
        {
            if (tags is null)
                throw new ArgumentNullException(nameof(tags));

            var holdingRegisters = new MTagsCollection();
            var inputRegisters = new MTagsCollection();
            var coils = new MTagsCollection();
            var discreteInputs = new MTagsCollection();

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
                IModbusMaster master = masterSource.Get(factory, FrameType, ConnectionSource);

                foreach (var g in (coils).Slice(MaxGroupLength, MaxGroupSpareLength))
                    await UpdateCoils(master, g).ConfigureAwait(false);

                foreach (var g in (discreteInputs).Slice(MaxGroupLength, MaxGroupSpareLength))
                    await UpdateDiscreteInputs(master, g).ConfigureAwait(false);

                foreach (var g in (inputRegisters).Slice(MaxGroupLength, MaxGroupSpareLength))
                    await UpdateInputRegisters(master, g).ConfigureAwait(false);

                foreach (var g in (holdingRegisters).Slice(MaxGroupLength, MaxGroupSpareLength))
                    await UpdateHoldingRegisters(master, g).ConfigureAwait(false);
            }

        }

        /// <summary>
        /// Записать значения тегов в устройство 
        /// </summary>
        /// <param name="tagsValues"></param>
        /// <returns>Count of written tags</returns>
        protected override async Task<int> Write(IDictionary<TagAbstract, object> tagsValues)
        {
            if (tagsValues is null)
                return 0;

            if(tagsValues.Count > 0)
            {
                var holdingRegisters = new MTagsCollection();
                var coils = new MTagsCollection();

                foreach (MTag tag in tagsValues.Keys)
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

                IModbusMaster master = masterSource.Get(factory, FrameType, ConnectionSource);

                foreach (var g in holdingRegisters.Slice())
                {
                    ushort[] values = new ushort[g.Length];
                    byte[] buff = new byte[32];

                    foreach (var t in g)
                    {
                        var v = tagsValues[t];
                        var tc = GetTypeConverter(t.ValueType);
                        
                        tc.GetBytes(buff, v);
                        var b = t.Begin - g.Begin;
                        for (int i = 0; i < t.Length; i++)
                            values[b + i] = BitConverter.ToUInt16(buff, i * 2);
                    }

                    await master.WriteMultipleRegistersAsync(SlaveId, (ushort)g.Begin, values).ConfigureAwait(false);
                }

                foreach (var g in coils.Slice())
                {
                    var values = new bool[g.Length];
                    int i = 0;
                    foreach (var t in g)
                        values[i++] = (bool)tagsValues[t];
                    await master.WriteMultipleCoilsAsync(SlaveId, (ushort)g.Begin, values).ConfigureAwait(false);
                }
            }

            return tagsValues.Count;
        }

        /// <summary>
        /// Функция синхронного чтения
        /// </summary>
        /// <param name="region">Region</param>
        /// <param name="begin">Start address</param>
        /// <param name="length">Count of registers.</param>
        /// <returns></returns>
        public async Task<ushort[]> ReadRegisters(ModbusRegion region, ushort begin, ushort length)
        {
            if (!ConnectionSource.IsOpen)
                throw new IOException("Device does not connected.");

            if (length > 125)
                throw new ArgumentException($"Length ({length}) is too big.");

            if (length == 0)
                throw new ArgumentException($"Length ({length}) is not good.");

            IModbusMaster master = masterSource.Get(factory, FrameType, ConnectionSource);

            switch (region)
            {
                case ModbusRegion.InputRegisters:
                    return await master.ReadInputRegistersAsync(SlaveId, begin, length).ConfigureAwait(false);
                case ModbusRegion.HoldingRegisters:
                    return await master.ReadHoldingRegistersAsync(SlaveId, begin, length).ConfigureAwait(false);
                case ModbusRegion.Coils:
                case ModbusRegion.DiscreteInputs:
                default:
                    throw new NotSupportedException($"{region} region does not supported by this function");
            }
        }

        /// <summary>
        /// Функция синхронной записи
        /// </summary>
        /// <param name="begin">Start address</param>
        /// <param name="data">registers.</param>
        /// <returns></returns>
        public async Task WriteRegisters(ushort begin, ushort[] data)
        {
            if (!ConnectionSource.IsOpen)
                throw new IOException("Device does not connected.");

            if (data.Length > 125)
                throw new ArgumentException($"Length ({data.Length}) is too big.");

            if (data.Length == 0)
                throw new ArgumentException($"Length ({data.Length}) is not good.");

            IModbusMaster master = masterSource.Get(factory, FrameType, ConnectionSource);

            await master.WriteMultipleRegistersAsync(SlaveId, begin, data).ConfigureAwait(false);
        }

        /// <summary>
        /// Функция синхронной записи и последующего чтения записанных значений
        /// </summary>
        /// <param name="begin">Start address</param>
        /// <param name="data">registers.</param>
        /// <returns></returns>
        public async Task<ushort[]> WriteAndReadRegisters(ushort begin, ushort[] data)
        {
            if (!ConnectionSource.IsOpen)
                throw new IOException("Device does not connected.");

            if (data?.Length > 125)
                throw new ArgumentException($"Length ({data.Length}) is too big.");

            if (data?.Length == 0)
                throw new ArgumentException($"Length ({data.Length}) is not good.");

            IModbusMaster master = masterSource.Get(factory, FrameType, ConnectionSource);

            await master.WriteMultipleRegistersAsync(SlaveId, begin, data).ConfigureAwait(false);

            return await master.ReadHoldingRegistersAsync(SlaveId, begin, (ushort)data.Length).ConfigureAwait(false);
        }
    }
}
