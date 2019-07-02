using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Modbus.TypeConverters
{
    public class TypeConverterInt32 : TypeConverterAbstract
    {
        public TypeConverterInt32(byte[] byteOrder) : base(byteOrder)
        {
        }

        public override int ByteLength => 4;
        
        public override void GetBytes(Span<byte> buffer, object value)
        {
            var buff = BitConverter.GetBytes((Int32)value);
            SetOrderedBuffer(buffer, buff);
        }
        
        public override object GetValue(Span<byte> buffer)
        {
            return BitConverter.ToInt32(GetOrderedBuffer(buffer));
        }
    }
}
