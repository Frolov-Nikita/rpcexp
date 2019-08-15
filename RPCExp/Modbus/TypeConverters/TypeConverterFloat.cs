using System;
using System.Collections.Generic;
using System.Text;
using RPCExp.Common;

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
            var buff = BitConverter.GetBytes((Single)Convert.ChangeType(value, typeof(Single)));
            SetOrderedBuffer(buffer, buff);
        }

        public override object GetValue(Span<byte> buffer)
        {
            return BitConverter.ToSingle(GetOrderedBuffer(buffer));
        }

    }
}
