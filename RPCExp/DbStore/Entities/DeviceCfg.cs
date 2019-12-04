using System;
using System.Collections.Generic;

namespace RPCExp.DbStore.Entities
{

    public class DeviceCfg : INameDescription, IProtocolSpecificData, ICopyFrom, IIdentity
    {
        public int Id { get; set; }

        public string ClassName { get; set; }

        public string Custom { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public long BadCommWaitPeriod { get; set; }

        public bool InActiveUpdate { get; set; }

        public long InActiveUpdatePeriod { get; set; }

        public ICollection<DeviceToTemplate> DeviceToTemplates { get; } = new List<DeviceToTemplate>();

        public ConnectionSourceCfg ConnectionSourceCfg { get; set; }

        public void CopyFrom(object original)
        {
            if (original is null)
                throw new ArgumentNullException(nameof(original));

            var src = (DeviceCfg)original;
            ClassName = src.ClassName;
            Custom = src.Custom;
            Name = src.Name;
            Description = src.Description;
            BadCommWaitPeriod = src.BadCommWaitPeriod;
            InActiveUpdate = src.InActiveUpdate;
            InActiveUpdatePeriod = src.InActiveUpdatePeriod;
            ConnectionSourceCfg = src.ConnectionSourceCfg;

            //foreach (var dtt in DeviceToTemplates)
            //    DeviceToTemplates.Add(dtt);
        }
    }
}