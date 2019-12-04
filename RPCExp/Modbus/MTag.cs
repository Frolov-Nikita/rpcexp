using RPCExp.Common;
using RPCExp.Modbus.TypeConverters;

namespace RPCExp.Modbus
{

    public class MTag : TagAbstract, IRange
    {
        private Access access = Access.ReadWrite;

        /// <summary>
        /// Разрешение на запись
        /// </summary>
        public override Access Access
        {
            get
            {
                var canWrite = ((Region == ModbusRegion.Coils) || (Region == ModbusRegion.HoldingRegisters)) && (access == Access.ReadWrite);

                if (canWrite)
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
