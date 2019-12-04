using System;

namespace RPCExp.Modbus.TypeConverters
{
    public class TypeConverterInt32 : TypeConverterAbstract
    {
        public TypeConverterInt32(byte[] byteOrder) : base(byteOrder)
        {
        }

        public override Common.ValueType ValueType => Common.ValueType.Int32;

        public override void GetBytes(Span<byte> buffer, object value)
        {

#pragma warning disable CA1305 // Укажите IFormatProvider
            var buff = BitConverter.GetBytes((int)Convert.ChangeType(value, typeof(int)));
#pragma warning restore CA1305 // Укажите IFormatProvider
            SetOrderedBuffer(buffer, buff);
        }

        public override object GetValue(Span<byte> buffer)
        {
            return BitConverter.ToInt32(GetOrderedBuffer(buffer));
        }
    }
}
