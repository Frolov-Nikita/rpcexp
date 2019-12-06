namespace RPCExp.Modbus
{
    /// <summary>
    /// regions of memory for modbus device
    /// </summary>
    public enum ModbusRegion
    {
        /// <summary>
        /// device logic bits (often digital outputs)
        /// </summary>
        Coils = 1,

        /// <summary>
        /// Digital inputs and read only device logic bits
        /// </summary>
        DiscreteInputs = 2,

        /// <summary>
        /// read only registers
        /// </summary>
        InputRegisters = 3,

        /// <summary>
        /// registers for reading and writing
        /// </summary>
        HoldingRegisters = 4,
    }
}
