using System;
using System.Collections.Generic;
using System.Text;
using RPCExp.Modbus;
using Newtonsoft.Json;

namespace RPCExp.Modbus.Factory
{
    public static class Factory
    {
        public static void SaveDevice(Device device, string file = "cfg.json")
        {
            var cfg = JsonConvert.SerializeObject(DeviceConfigWrapper.From(device), Formatting.Indented);
            System.IO.File.WriteAllText(file, cfg);
        }

        public static Device LoadDevice(string file)
        {
            var cfg = System.IO.File.ReadAllText(file);
            return JsonConvert.DeserializeObject<DeviceConfigWrapper>(cfg)?.Get();
        }

        public class MTagConfigWrapper
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public int Begin { get; set; }

            public bool CanWrite { get; set; }

            public ModbusRegion Region { get; set; }

            public TypeConverters.ModbusValueType ValueType { get; set; }

            public MTag Get() =>
                new MTag
                {
                    Name = this.Name,
                    Description = this.Description,
                    Begin = this.Begin,
                    CanWrite = this.CanWrite,
                    Region = this.Region,
                    ValueType = this.ValueType,
                };

            public static MTagConfigWrapper From(MTag mTag) =>
                new MTagConfigWrapper
                {
                    Name = mTag.Name,
                    Description = mTag.Description,
                    Begin = mTag.Begin,
                    CanWrite = mTag.CanWrite,
                    Region = mTag.Region,
                    ValueType = mTag.ValueType,
                };
            
        }

        public class DeviceConfigWrapper
        {
            public string Name { get; set; }
            public string Description { get; set; }

            public List<MTagConfigWrapper> Tags { get; set; }

            public byte SlaveId { get; set; }

            public string Host { get; set; }

            public int Port { get; set; }

            public long BadCommWaitPeriod { get; set; }

            public bool InActiveUpdate { get; set; }

            public long InActiveUpdatePeriod { get; set; }

            public byte[] ByteOrder { get; set; }

            public Device Get()
            {
                var d = new Device
                {
                    Name = this.Name,
                    Description = this.Description,
                    SlaveId = this.SlaveId,
                    Host = this.Host,
                    Port = this.Port,
                    BadCommWaitPeriod = this.BadCommWaitPeriod,
                    InActiveUpdate = this.InActiveUpdate,
                    InActiveUpdatePeriod = this.InActiveUpdatePeriod,
                    ByteOrder = this.ByteOrder,
                };
                foreach (var t in Tags)
                    d.Tags.Add(t.Name, t.Get());
                return d;
            }

            public static DeviceConfigWrapper From(Device device)
            {
                var d = new DeviceConfigWrapper
                {
                    Name = device.Name,
                    Description = device.Description,
                    SlaveId = device.SlaveId,
                    Host = device.Host,
                    Port = device.Port,
                    BadCommWaitPeriod = device.BadCommWaitPeriod,
                    InActiveUpdate = device.InActiveUpdate,
                    InActiveUpdatePeriod = device.InActiveUpdatePeriod,
                    ByteOrder = device.ByteOrder,
                };
                d.Tags= new List<MTagConfigWrapper>();
                foreach (var t in device.Tags.Values)
                    d.Tags.Add(MTagConfigWrapper.From((MTag)t));

                return d;

            }
        }

    }
}
