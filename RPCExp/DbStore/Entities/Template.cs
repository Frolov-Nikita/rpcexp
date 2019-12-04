using System;
using System.Collections.Generic;

namespace RPCExp.DbStore.Entities
{
    public class Template : INameDescription, ICopyFrom, IIdentity
    {
        public int Id { get; set; }

        public string Name { get; set; } = Guid.NewGuid().ToString();

        public string Description { get; set; } = "";

        public List<TagCfg> Tags { get; } = new List<TagCfg>();

        public List<AlarmCfg> Alarms { get; } = new List<AlarmCfg>();

        public List<ArchiveCfg> Archives { get; } = new List<ArchiveCfg>();

        public ICollection<DeviceToTemplate> DeviceToTemplates { get; } = new List<DeviceToTemplate>();

        public void CopyFrom(object original)
        {
            if (original is null)
                throw new ArgumentNullException(nameof(original));

            var src = (Template)original;

            Name = src.Name;
            Description = src.Description;

            foreach (var t in src.Tags)
                Tags.Add(t);

            foreach (var a in src.Alarms)
                Alarms.Add(a);

            foreach (var z in src.Archives)
                Archives.Add(z);

            foreach (var d in src.DeviceToTemplates)
                DeviceToTemplates.Add(d);
        }
    }
}
