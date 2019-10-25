using RPCExp.Modbus;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RPCExp.Store.Entities
{

    public class DeviceCfg : IProtocolSpecificData
    {
        public string ClassName { get; set; }

        public string Custom { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }

        public long BadCommWaitPeriod { get; set; }

        public bool InActiveUpdate { get; set; }

        public long InActiveUpdatePeriod { get; set; }
        
        public ICollection<Template> Templates { get; set; }

    }
}