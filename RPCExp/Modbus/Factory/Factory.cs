using System;
using System.Collections.Generic;
using System.Text;
using RPCExp.Modbus;
using Newtonsoft.Json;
using RPCExp.Modbus.TypeConverters;
using RPCExp.Common;

namespace RPCExp.Modbus.Factory
{
    public static class Factory
    {
        public static void SaveFacility(Facility obj, string file = "cfg.json")
        {
            var dcw = new FacilityCfgWrapper();
            dcw.Wrap(obj);
            var cfg = JsonConvert.SerializeObject(dcw, Formatting.Indented);
            System.IO.File.WriteAllText(file, cfg);
        }

        public static Facility LoadFacility(string file)
        {
            var cfg = System.IO.File.ReadAllText(file);
            return JsonConvert.DeserializeObject<FacilityCfgWrapper>(cfg)?.Unwrap();
        }
    }

    public interface IClassWrapper<T>
    {
        string ClassName { get; }

        void Wrap(T obj);

        T Unwrap();
    }
    
    public class MTagCfgWrapper : IClassWrapper<MTag>
    {
        public string ClassName => nameof(MTag);

        public string Name { get; set; }

        public string Description { get; set; }

        public int Begin { get; set; }

        public bool CanWrite { get; set; }

        public ModbusRegion Region { get; set; }

        public ModbusValueType ValueType { get; set; }
        
        public MTag Unwrap() =>
            new MTag
            {
                Name = this.Name,
                Description = this.Description,
                Begin = this.Begin,
                CanWrite = this.CanWrite,
                Region = this.Region,
                ValueType = this.ValueType,
            };

        public void Wrap(MTag obj)
        {
            Name = obj.Name;
            Description = obj.Description;
            Begin = obj.Begin;
            CanWrite = obj.CanWrite;
            Region = obj.Region;
            ValueType = obj.ValueType;
        }
    }

    public class DeviceCfgWrapper : IClassWrapper<Device>
    {
        public string ClassName => nameof(Device);

        public string Name { get; set; }

        public string Description { get; set; }

        public List<MTagCfgWrapper> Tags { get; set; }

        public byte SlaveId { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public long BadCommWaitPeriod { get; set; }

        public bool InActiveUpdate { get; set; }

        public long InActiveUpdatePeriod { get; set; }

        public byte[] ByteOrder { get; set; }

        public string DevType => "ModbusTCP";
        
        public void Wrap(Device obj)
        {
            Name = obj.Name;
            Description = obj.Description;
            SlaveId = obj.SlaveId;
            Host = obj.Host;
            Port = obj.Port;
            BadCommWaitPeriod = obj.BadCommWaitPeriod;
            InActiveUpdate = obj.InActiveUpdate;
            InActiveUpdatePeriod = obj.InActiveUpdatePeriod;
            ByteOrder = obj.ByteOrder;
            Tags = new List<MTagCfgWrapper>();
            foreach (MTag t in obj.Tags.Values)
            {
                var tagWraped = new MTagCfgWrapper();
                tagWraped.Wrap(t);
                Tags.Add(tagWraped);
            }
                
        }

        public Device Unwrap()
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
                d.Tags.Add(t.Name, t.Unwrap());

            return d;
        }
    }

    public class FacilityCfgWrapper : IClassWrapper<Facility>
    {
        public string ClassName => nameof(Facility);

        public string Name { get; set; }

        public string Description { get; set; }

        public List<DeviceCfgWrapper> Devices { get; set; }

        public Facility Unwrap()
        {
            var f = new Facility
            {
                Name = this.Name,
                Description = this.Description,
            };
            f.Devices = new List<DeviceAbstract>();

            foreach (var d in Devices)
                f.Devices.Add(d.Unwrap());

            return f;
        }

        public void Wrap(Facility obj)
        {
            Name = obj.Name;
            Description = obj.Description;
            Devices = new List<DeviceCfgWrapper>(obj.Devices.Count);
            foreach (var d in obj.Devices)
            {
                var dev = new DeviceCfgWrapper();
                dev.Wrap((Device)d);
                Devices.Add(dev);
            }
        }
    }

}
