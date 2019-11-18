using Newtonsoft.Json;
using RPCExp.Common;
using RPCExp.Modbus.TypeConverters;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RPCExp.Modbus
{

    public class MTag : TagAbstract, IRange
    {
        private Access access;

        //### специфично только для modbus
        public override Access Access
        {
            get
            {
                // TODO: обработать access
                if ((Region == ModbusRegion.Coils) || (Region == ModbusRegion.HoldingRegisters))
                    return Access.ReadWrite;
                else
                    return Access.ReadOnly;
            }
            set => access = value;
        }

        public ModbusRegion Region { get; set; }

        public int Begin { get; set; }

        public int Length => TypeConverterAbstract.GetByteLength(ValueType) / 2;

        public int End => Begin + (Length - 1);

        //### специфично только для modbus */
    }

}
