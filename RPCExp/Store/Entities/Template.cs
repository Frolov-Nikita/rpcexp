using RPCExp.AlarmLogger.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Store.Entities
{
    public class Template: INameDescription
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<TagCfg> Tags { get; set; }

        public List<AlarmCfg> Alarms { get; set; }

        public List<ArchiveCfg> Archives { get; set; }
                
    }
}
