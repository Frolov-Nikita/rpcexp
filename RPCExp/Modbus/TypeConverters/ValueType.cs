using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Modbus.TypeConverters
{
    public enum ModbusValueType: byte
    {
        Bool,
        Float,
        Int16,
        Int32,
    }
}
