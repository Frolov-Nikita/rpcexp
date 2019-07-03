using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Modbus.TypeConverters
{
    public class TypeConverterInt16 : TypeConverterAbstract
    {
        public TypeConverterInt16(byte[] byteOrder) : base(byteOrder)
        {
        }

        public override ModbusValueType ValueType => ModbusValueType.Int16;

        public override void GetBytes(Span<byte> buffer, object value)
        {
            var buff = BitConverter.GetBytes((Int16)value);
            SetOrderedBuffer(buffer, buff);
        }

        public override object GetValue(Span<byte> buffer)
        {
            return BitConverter.ToInt16(GetOrderedBuffer(buffer));
        }
    }
}
