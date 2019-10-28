using RPCExp.Common;
using RPCExp.Modbus;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RPCExp.Store.Entities
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

        public ICollection<DeviceToTemplate> Templates { get; set; } = new List<DeviceToTemplate>();

        public void CopyFrom(object original)
        {
            var src = (DeviceCfg)original;
            ClassName = src.ClassName;
            Custom = src.Custom;
            Name = src.Name;
            Description = src.Description;
            BadCommWaitPeriod = src.BadCommWaitPeriod;
            InActiveUpdate = src.InActiveUpdate;
            InActiveUpdatePeriod = src.InActiveUpdatePeriod;

            foreach (var dtt in Templates)
                Templates.Add(dtt);
        }
    }
}