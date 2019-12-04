using System;

namespace RPCExp.Modbus.TypeConverters
{
    public class TypeConverterFloat : TypeConverterAbstract
    {
        public TypeConverterFloat(byte[] byteOrder) : base(byteOrder)
        {
        }

        public override Common.ValueType ValueType => Common.ValueType.Float;

        public override void GetBytes(Span<byte> buffer, object value)
        {
#pragma warning disable CA1305 // Укажите IFormatProvider
            var buff = BitConverter.GetBytes((float)Convert.ChangeType(value, typeof(float)));
#pragma warning restore CA1305 // Укажите IFormatProvider
            SetOrderedBuffer(buffer, buff);
        }

        public override object GetValue(Span<byte> buffer)
        {
            return BitConverter.ToSingle(GetOrderedBuffer(buffer));
        }

    }
}
