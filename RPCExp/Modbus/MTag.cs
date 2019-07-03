using Newtonsoft.Json;
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

        public ModbusValueType ValueType { get; set; }

        public int Begin { get; set; }

        [JsonIgnore]
        public int Length => TypeConverterAbstract.GetByteLength(ValueType) / 2;

        [JsonIgnore]
        public int End => Begin + (Length - 1);
        //### специфично только для modbus */

    }

}
