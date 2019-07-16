using System;
using System.Collections.Generic;
using System.Text;

namespace Sim
{
    public enum ModbusRegion: byte
    {
        Coils = 1,
        DiscreteInputs = 2,
        InputRegisters = 3,
        HoldingRegisters = 4,
    }
}
