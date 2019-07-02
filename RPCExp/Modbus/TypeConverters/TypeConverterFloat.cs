using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Modbus.TypeConverters
{
    public class TypeConverterFloat : TypeConverterAbstract
    {
        public TypeConverterFloat(byte[] byteOrder) : base(byteOrder)
        {
        }

        public override int ByteLength => 4;
        
        public override void GetBytes(Span<byte> buffer, object value)
        {
            var buff = BitConverter.GetBytes((Single)value);
            SetOrderedBuffer(buffer, buff);
        }

        public override object GetValue(Span<byte> buffer)
        {
            return BitConverter.ToSingle(GetOrderedBuffer(buffer));
        }
    }
}
