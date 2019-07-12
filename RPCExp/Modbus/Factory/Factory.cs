using System;
using System.Collections.Generic;
using System.Linq;
using RPCExp.Modbus;
using Newtonsoft.Json;
using RPCExp.Modbus.TypeConverters;
using RPCExp.Common;

namespace RPCExp.Modbus.Factory
{
    public static class Factory
    {
        static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            //TODO ! Избегать такого при помощи сервисов-ресурсов.
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.Indented,
        };

        public static void SaveFacility(Facility obj, string file = "cfg.json")
        {
            var dcw = new FacilityCfgWrapper();
            dcw.Wrap(obj);
            var cfg = JsonConvert.SerializeObject(dcw, jsonSerializerSettings);
            System.IO.File.WriteAllText(file, cfg);
        }

        public static Facility LoadFacility(string file)
        {
            var cfg = System.IO.File.ReadAllText(file);
            return JsonConvert.DeserializeObject<FacilityCfgWrapper>(cfg, jsonSerializerSettings)?.Unwrap();
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

        public ICollection<MTagCfgWrapper> Tags { get; set; }

        public byte SlaveId { get; set; }
        
        public long BadCommWaitPeriod { get; set; }

        public bool InActiveUpdate { get; set; }

        public long InActiveUpdatePeriod { get; set; }

        public byte[] ByteOrder { get; set; }

        public string FrameType { get; set; }

        public string ConnectionCfg { get; set; }

        public void Wrap(Device obj)
        {
            Name = obj.Name;
            Description = obj.Description;
            SlaveId = obj.SlaveId;
            BadCommWaitPeriod = obj.BadCommWaitPeriod;
            InActiveUpdate = obj.InActiveUpdate;
            InActiveUpdatePeriod = obj.InActiveUpdatePeriod;
            ByteOrder = obj.ByteOrder;
            FrameType = obj.MasterSource.frameType.ToString();
            ConnectionCfg = obj.Connection.ConnectionCfg;
            
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

            if (!Enum.TryParse(typeof(FrameType), FrameType, out object ft))
                throw new Exception($"FrameType: {FrameType} is invalid");

            var obj = new Device
            {
                Name = this.Name,
                Description = this.Description,
                SlaveId = this.SlaveId,
                BadCommWaitPeriod = this.BadCommWaitPeriod,
                InActiveUpdate = this.InActiveUpdate,
                InActiveUpdatePeriod = this.InActiveUpdatePeriod,
                ByteOrder = this.ByteOrder,
                MasterSource = new MasterSource { frameType = (FrameType)ft},
            };
            foreach (var t in Tags)
                obj.Tags.Add(t.Name, t.Unwrap());

            return obj;
        }
    }

    public class FacilityCfgWrapper : IClassWrapper<Facility>
    {
        public string ClassName => nameof(Facility);

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<ConnectionSource> Connections { get; set; }

        public ICollection<DeviceCfgWrapper> Devices { get; set; }


        public Facility Unwrap()
        {
            var obj = new Facility
            {
                Name = this.Name,
                Description = this.Description,
            };

            obj.DevicesSource = Devices.ToDictionary(d=>d.Name, d=> (DeviceAbstract)d.Unwrap());

            obj.ConnectionsSource = Connections.ToDictionary(c=>c.Name, c=>c);

            return obj;
        }

        public void Wrap(Facility obj)
        {
            Name = obj.Name;
            Description = obj.Description;
            Devices = new List<DeviceCfgWrapper>(obj.DevicesSource.Count);
            foreach (var d in obj.DevicesSource.Values)
            {
                var dev = new DeviceCfgWrapper();
                dev.Wrap((Device)d);
                Devices.Add(dev);
            }
            Connections = obj.ConnectionsSource.Values;
        }
    }

}
