using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Modbus
{
    public enum ModbusRegion: byte
    {
        Coils = 1,
        DiscreteInputs = 2,
        InputRegisters = 3,
        HoldingRegisters = 4,
    }
}
