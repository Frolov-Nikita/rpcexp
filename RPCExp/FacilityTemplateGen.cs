using RPCExp.Common;
using RPCExp.Modbus;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp
{
    public class FacilityTemplateGen
    {
        public static Facility GetFacility()
        {
            var conn = new ConnectionSource
            {
                ConnectionCfg = "127.0.0.1:11502",
                Physic = Physic.Tcp,
                Name = "localhost"
            };

            var facility = new Facility
            {
                ConnectionsSource = new Dictionary<string, ConnectionSource>
                {
                    [conn.Name] = conn,
                },

                DevicesSource = new Dictionary<string, DeviceAbstract>
                {
                    ["Plc1"] = new Device
                    {
                        Name = "Plc1",
                        Connection = conn,
                        MasterSource = new MasterSource
                        {
                            frameType = FrameType.Ip,
                        },
                        SlaveId = 1,
                    }
                }
            };

            var dev = facility.DevicesSource["Plc1"];

            dev.Tags.Add("Tag1", new MTag
            {
                Name = "Tag1",
                Region = ModbusRegion.HoldingRegisters,
                Begin = 0,
                ValueType = Modbus.TypeConverters.ModbusValueType.Int16,
            });

            dev.Tags.Add("Tag2", new MTag
            {
                Name = "Tag2",
                Region = ModbusRegion.HoldingRegisters,
                Begin = 1,
                ValueType = Modbus.TypeConverters.ModbusValueType.Int16,
            });

            dev.Tags.Add("Tag3", new MTag
            {
                Name = "Tag3",
                Region = ModbusRegion.HoldingRegisters,
                Begin = 2,
                ValueType = Modbus.TypeConverters.ModbusValueType.Int16,
            });

            dev.Tags.Add("Tag4", new MTag
            {
                Name = "Tag4",
                Region = ModbusRegion.HoldingRegisters,
                Begin = 3,
                ValueType = Modbus.TypeConverters.ModbusValueType.Float,
            });

            dev.Tags.Add("boolTag5", new MTag
            {
                Name = "boolTag5",
                Region = ModbusRegion.Coils,
                Begin = 3,
                ValueType = Modbus.TypeConverters.ModbusValueType.Bool,
            });

            return facility;
        }
    }
}
