using RPCExp.Common;
using RPCExp.Modbus.TypeConverters;

namespace RPCExp.Modbus
{
    /// <summary>
    /// Modbus implementation of tag.
    /// Additionally contains modbus related parameters
    /// </summary>
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

        /// <summary>
        /// region of memory of device contains this tag
        /// </summary>
        public ModbusRegion Region { get; set; }

        /// <summary>
        /// Address of first modbus register of this tag.
        /// </summary>
        public int Begin { get; set; }

        /// <summary>
        /// count of registers of this tag
        /// </summary>
        public int Length => TypeConverterAbstract.GetByteLength(ValueType) / 2;

        /// <summary>
        /// Address of last modbus register of this tag.
        /// </summary>
        public int End => Begin + (Length - 1);

        //### специфично только для modbus */
    }

}
