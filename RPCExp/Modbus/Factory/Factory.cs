using System;
using System.Collections.Generic;
using System.Linq;
using RPCExp.Modbus;
using Newtonsoft.Json;
using RPCExp.Modbus.TypeConverters;
using RPCExp.Common;
using System.Collections;

namespace RPCExp.Modbus.Factory
{
    public static class Factory
    {
        static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            // TODO ! Избегать такого при помощи сервисов-ресурсов.
            // PreserveReferencesHandling = PreserveReferencesHandling.Objects,
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

        public string[] Sets { get; set; }

        public int Begin { get; set; }

        public bool CanWrite { get; set; }

        public ModbusRegion Region { get; set; }

        public ModbusValueType ValueType { get; set; }

        public void Wrap(MTag obj)
        {
            Name = obj.Name;
            Description = obj.Description;
            Begin = obj.Begin;
            CanWrite = obj.CanWrite;
            Region = obj.Region;
            ValueType = obj.ValueType;
            Sets = obj.Sets.Keys?.ToArray();
        }

        public MTag Unwrap() =>
            new MTag
            {
                Name = this.Name,
                Description = this.Description,
                Begin = this.Begin,
                CanWrite = this.CanWrite,
                Region = this.Region,
                ValueType = this.ValueType,
                Sets = new Dictionary<string, TagsSet>(),
            };
    }

    public class DeviceCfgWrapper : IClassWrapper<Device>
    {
        public string ClassName => nameof(Device);

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<MTagCfgWrapper> Tags { get; set; }

        public ICollection<TagsSet> Sets { get; set; }

        public byte SlaveId { get; set; }
        
        public long BadCommWaitPeriod { get; set; }

        public bool InActiveUpdate { get; set; }

        public long InActiveUpdatePeriod { get; set; }

        public byte[] ByteOrder { get; set; }

        public string FrameType { get; set; }

        public string ConnectionCfg { get; set; }

        class TagsSetComparer : IEqualityComparer<TagsSet>
        {
            public bool Equals(TagsSet x, TagsSet y) =>  
                x.Name == y.Name;
            
            public int GetHashCode(TagsSet obj)=>
                obj.Name.GetHashCode();
        }

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
            ConnectionCfg = obj.Connection.Cfg;
            Sets = new List<TagsSet>(obj.Sets.Count);
            Tags = new List<MTagCfgWrapper>(obj.Tags.Count);
            
            foreach (var s in obj.Sets.Values)
            {
                var tagsSet = new TagsSet
                {
                    Name = s.Name,
                    Description = s.Description
                };
                Sets.Add(tagsSet);
            }

            foreach (MTag t in obj.Tags.Values)
            {
                var tagWraped = new MTagCfgWrapper();
                tagWraped.Wrap(t);
                Tags.Add(tagWraped);

                foreach (var s in t.Sets.Values)
                    if (!obj.Sets.ContainsKey(s.Name))
                        Sets.Add(new TagsSet {
                            Name = s.Name,
                            Description = s.Description
                        });
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
                MasterSource = new MasterSource { frameType = (FrameType)ft },
                Sets = new Dictionary<string, TagsSet>(),
                //Connection = null,
            };

            if((Sets?.Count??0) > 0)
                foreach (var s in Sets)
                    obj.Sets.Add(s.Name, new TagsSet(s));

            var unUsed = new TagsSet { Name = "UnUsed", Description = "Tag config does not point the tagsSet" };

            foreach (var t in Tags)
            {
                var tag = t.Unwrap();

                if((t.Sets?.Length??0) > 0)
                    foreach (var setName in t.Sets)
                    {
                        if (!obj.Sets.ContainsKey(setName))
                            obj.Sets.Add(setName, new TagsSet { Name = setName });

                        tag.Sets.Add(setName, obj.Sets[setName]);
                    }
                else
                {
                    if (!obj.Sets.ContainsKey(unUsed.Name))
                        obj.Sets.Add(unUsed.Name, unUsed);
                    tag.Sets.Add(unUsed.Name, unUsed);
                }
                    
                obj.Tags.Add(t.Name, tag);
            }

            return obj;
        }
    }

    public class FacilityCfgWrapper : IClassWrapper<Facility>
    {
        public string ClassName => nameof(Facility);

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<string> Connections { get; set; }

        public ICollection<DeviceCfgWrapper> Devices { get; set; }

        public Facility Unwrap()
        {
            var obj = new Facility
            {
                Name = this.Name,
                Description = this.Description,
            };

            obj.ConnectionsSource = new Dictionary<string, ConnectionSource>();
            foreach (var c in Connections)
            {
                var cs = new ConnectionSource(c);
                obj.ConnectionsSource.Add(cs.Cfg, cs);
            }

            obj.DevicesSource = Devices.ToDictionary(d=>d.Name, d=> {
                var dev = d.Unwrap();
                if (obj.ConnectionsSource.TryGetValue(d.ConnectionCfg, out ConnectionSource cs))
                    dev.Connection = cs;
                else
                {
                    var csnew = new ConnectionSource(d.ConnectionCfg);
                    obj.ConnectionsSource.Add(csnew.Cfg, csnew);
                    dev.Connection = csnew;
                }
                return (DeviceAbstract)dev;
            });



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
            Connections = obj.ConnectionsSource.Values.Select(v=>v.Cfg).ToList();
        }
    }

}
