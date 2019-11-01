using RPCExp.AlarmLogger.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Store.Entities
{
    public class Template: INameDescription, ICopyFrom, IIdentity
    {
        public int Id { get; set; }

        public string Name { get; set; } = Guid.NewGuid().ToString();

        public string Description { get; set; } = "";

        public List<TagCfg> Tags { get; set; } = new List<TagCfg>();

        public List<AlarmCfg> Alarms { get; set; } = new List<AlarmCfg>();

        public List<ArchiveCfg> Archives { get; set; } = new List<ArchiveCfg>();

        public ICollection<DeviceToTemplate> DeviceToTemplates { get; set; } = new List<DeviceToTemplate>();

        public void CopyFrom(object original)
        {
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
