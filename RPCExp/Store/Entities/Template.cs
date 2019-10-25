using RPCExp.AlarmLogger.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Store.Entities
{
    public class Template: INameDescription
    {
        public int Id { get; set; }

        public string Name { get; set; } = Guid.NewGuid().ToString();

        public string Description { get; set; } = "";

        public List<TagCfg> Tags { get; set; } = new List<TagCfg>();

        public List<AlarmCfg> Alarms { get; set; } = new List<AlarmCfg>();

        public List<ArchiveCfg> Archives { get; set; } = new List<ArchiveCfg>();

        public List<DeviceCfg> Devices { get; set; } = new List<DeviceCfg>();
    }
}
