using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NModbus;
using RPCExp.Common;

namespace RPCExp.Modbus
{

    public interface IDriver
    {
        byte[] Read(object destination);

        bool Write(object destination, byte[] values);
    }

    public interface ModbusTCPDriver
    {
        byte[] Read(
            ModbusRegion region,
            byte slaveId,
            ushort address,
            ushort length
            );

        bool Write(
            ModbusRegion region,
            byte slaveId,
            ushort address,

            byte[] values
            );

    }


}
