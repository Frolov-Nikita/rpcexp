using RPCExp.Common;
using RPCExp.Modbus.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RPCExp.Modbus
{

    public class MTag : TagAbstract, IRange
    {
        private bool canWrite;

        public override bool CanRead { get; set; }

        public override bool CanWrite
        {
            get
            {
                if ((Region == ModbusRegion.Coils) || (Region == ModbusRegion.HoldingRegisters))
                    return canWrite;
                else
                    return false;
            }
            set => canWrite = value;
        }

        //### специфично только для modbus
        public ModbusRegion Region { get; set; }

        public int Begin { get; set; }

        public int Length => TypeConv.ByteLength / 2;

        public int End => Begin + (Length - 1);
        //### специфично только для modbus */

        public TypeConverterAbstract TypeConv { get; set; }
        
        internal void SetValue(Span<byte> data) =>
            SetValue(TypeConv.GetValue(data));
    }

}
