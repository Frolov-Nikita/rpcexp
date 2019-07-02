using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common.TypeConverters
{
    public class TypeConverterBool : TypeConverterAbstract
    {
        public TypeConverterBool(byte[] byteOrder) : base(byteOrder)
        {
        }

        public override int ByteLength => 2;
        
        public override void GetBytes(Span<byte> buffer, object value)
        {
            var buff = BitConverter.GetBytes((Int16)value);
            buffer[ByteOrder[0]] = buff[0];
            buffer[ByteOrder[1]] = buff[1];
        }

        public override object GetValue(Span<byte> buffer)
        {
            return buffer[0] > 0;
        }
    }
}
