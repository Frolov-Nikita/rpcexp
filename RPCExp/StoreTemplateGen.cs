using RPCExp.Common;
using RPCExp.Connections;
using RPCExp.Modbus;
using System;
using System.Collections.Generic;

namespace RPCExp
{
    public static class StoreTemplateGen
    {
        public static Store Get()
        {
            var store = new Store();

            var connectionSource = new TcpConnectionSource
            {
                Port = 11502,
                Host = "127.0.0.1",
                Name = "localhost Tests",
            };

            store.ConnectionsSources.AddByName(connectionSource);

            var facility = new Facility
            {
                Name = "TestFacility",
                AccessName = "orgName$departmentName$FieldName$wellName",
            };

            store.Facilities.AddByName(facility);



            var device = new ModbusDevice
            {
                Name = "Plc1",
                FrameType = FrameType.Ip,
                SlaveId = 1,
            };

            facility.Devices.AddByName(device);

            device.ConnectionSource = connectionSource;

            var tagGroup1 = new TagsGroup { Name = "currentData", Min = 10_000_000 };
            var tagGroup2 = new TagsGroup { Name = "settings", Min = 10_000_000 };


            var tag1 = new MTag
            {
                Groups = new Dictionary<string, TagsGroup>
                {
                    [tagGroup1.Name] = tagGroup1,
                },
                TemplateId = 1,
                Name = "Tag1",
                Region = ModbusRegion.HoldingRegisters,
                Begin = 0,
                ValueType = Common.ValueType.Int16,

            };

            device.Tags.Add("Tag1", tag1);

            device.Tags.Add("Tag2", new MTag
            {
                Groups = new Dictionary<string, TagsGroup>
                {
                    [tagGroup1.Name] = tagGroup1,
                    [tagGroup2.Name] = tagGroup2,
                },
                TemplateId = 1,
                Name = "Tag2",
                Region = ModbusRegion.HoldingRegisters,
                Begin = 1,
                ValueType = Common.ValueType.Int16,
            });

            device.Tags.Add("Tag3", new MTag
            {
                Groups = new Dictionary<string, TagsGroup>
                {
                    [tagGroup1.Name] = tagGroup1,
                },
                TemplateId = 2,
                Name = "Tag3",
                Region = ModbusRegion.HoldingRegisters,
                Begin = 2,
                ValueType = Common.ValueType.Int16,
            });

            device.Tags.Add("Tag4", new MTag
            {
                Groups = new Dictionary<string, TagsGroup>
                {
                    [tagGroup1.Name] = tagGroup1,
                },
                TemplateId = 2,
                Name = "Tag4",
                Region = ModbusRegion.HoldingRegisters,
                Begin = 3,
                ValueType = Common.ValueType.Float,
            });

            device.Tags.Add("boolTag5", new MTag
            {
                Groups = new Dictionary<string, TagsGroup>
                {
                    [tagGroup1.Name] = tagGroup1,
                },
                TemplateId = 1,
                Name = "boolTag5",
                Region = ModbusRegion.Coils,
                Begin = 3,
                ValueType = Common.ValueType.Bool,
            });

            store.TagLogService.Configs.Add(new TagLogger.TagLogConfig(tag1)
            {
                Hyst = 1,
                PeriodMaxSec = 600,
                PeriodMinSec = 1,
                TagLogInfo = new TagLogger.Entities.TagLogInfo
                {
                    DeviceName = device.Name,
                    FacilityAccessName = facility.AccessName,
                    TagName = tag1.Name,
                }
            });


            return store;
        }
    }
}
