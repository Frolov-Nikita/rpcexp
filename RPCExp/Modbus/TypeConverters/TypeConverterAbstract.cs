using System;

namespace RPCExp.Modbus.TypeConverters
{
    public abstract class TypeConverterAbstract
    {
        public TypeConverterAbstract(byte[] byteOrder)
        {
            if (byteOrder is null)
                throw new ArgumentNullException(nameof(byteOrder));

            ByteOrder = new byte[ByteLength];
            byte min = 0xFF;
            for (var i = 0; i < ByteLength; i++)
            {
                ByteOrder[i] = byteOrder[i];
                if (min > byteOrder[i])
                    min = byteOrder[i];
            }
            for (var i = 0; i < ByteLength; i++)
                ByteOrder[i] -= min;
        }

        public int ByteLength => GetByteLength(ValueType);

        public abstract Common.ValueType ValueType { get; }

#pragma warning disable CA1819 // Свойства не должны возвращать массивы
        protected byte[] ByteOrder { get; set; }
#pragma warning restore CA1819 // Свойства не должны возвращать массивы

        protected void SetOrderedBuffer(Span<byte> dest, Span<byte> src)
        {
            for (int i = 0; i < ByteLength; i++)
                dest[ByteOrder[i]] = src[i];
        }

        protected Span<byte> GetOrderedBuffer(Span<byte> buffer)
        {
            var buf = new byte[ByteLength];
            for (int i = 0; i < ByteLength; i++)
                buf[ByteOrder[i]] = buffer[i];
            return buf.AsSpan();
        }

        public abstract object GetValue(Span<byte> buffer);

        public abstract void GetBytes(Span<byte> buffer, object value);

        public static int GetByteLength(Common.ValueType modbusValueType)
        {
            switch (modbusValueType)
            {
                case Common.ValueType.Bool:
                    return 2;
                case Common.ValueType.Float:
                    return 4;
                case Common.ValueType.Int16:
                    return 2;
                case Common.ValueType.Int32:
                    return 4;
                default:
                    return 2;
            }
        }
    }
}