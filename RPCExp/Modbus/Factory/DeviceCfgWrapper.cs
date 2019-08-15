using System;
using System.Collections.Generic;
using System.Linq;
using RPCExp.Common;

namespace RPCExp.Modbus.Factory
{
    public class DeviceCfgWrapper : IClassWrapper<Device>
    {
        public string ClassName => nameof(Device);

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<MTagCfgWrapper> Tags { get; set; }

        public ICollection<TagsGroup> Groups { get; set; }

        public byte SlaveId { get; set; }
        
        public long BadCommWaitPeriod { get; set; }

        public bool InActiveUpdate { get; set; }

        public long InActiveUpdatePeriod { get; set; }

        public byte[] ByteOrder { get; set; }

        public string FrameType { get; set; }

        public string ConnectionCfg { get; set; }

        class TagsSetComparer : IEqualityComparer<TagsGroup>
        {
            public bool Equals(TagsGroup x, TagsGroup y) =>  
                x.Name == y.Name;
            
            public int GetHashCode(TagsGroup obj)=>
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
            Groups = new List<TagsGroup>(obj.Groups.Count);
            Tags = new List<MTagCfgWrapper>(obj.Tags.Count);
            
            foreach (var s in obj.Groups.Values)
            {
                var tagsSet = new TagsGroup
                {
                    Name = s.Name,
                    Description = s.Description
                };
                Groups.Add(tagsSet);
            }

            foreach (MTag t in obj.Tags.Values)
            {
                var tagWraped = new MTagCfgWrapper();
                tagWraped.Wrap(t);
                Tags.Add(tagWraped);

                foreach (var s in t.Groups.Values)
                    if (!obj.Groups.ContainsKey(s.Name))
                        Groups.Add(new TagsGroup {
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
                Groups = new Dictionary<string, TagsGroup>(),
                //Connection = null,
            };

            if((Groups?.Count??0) > 0)
                foreach (var s in Groups)
                    obj.Groups.Add(s.Name, new TagsGroup(s));

            var unUsed = new TagsGroup { Name = "UnUsed", Description = "Tag config does not point the tagsSet" };

            foreach (var t in Tags)
            {
                var tag = t.Unwrap();

                if((t.Groups?.Length??0) > 0)
                    foreach (var group in t.Groups)
                    {
                        if (!obj.Groups.ContainsKey(group))
                            obj.Groups.Add(group, new TagsGroup { Name = group });

                        tag.Groups.Add(group, obj.Groups[group]);
                    }
                else
                {
                    if (!obj.Groups.ContainsKey(unUsed.Name))
                        obj.Groups.Add(unUsed.Name, unUsed);
                    tag.Groups.Add(unUsed.Name, unUsed);
                }
                    
                obj.Tags.Add(t.Name, tag);
            }

            return obj;
        }
    }

}
