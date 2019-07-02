using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RPCExp.Common
{

    public abstract class DeviceAbstract : ServiceAbstract, IDevice
    {
        public virtual string Name { get ; set ; }

        public virtual IDictionary<string, TagAbstract> Tags { get; } = new Dictionary<string, TagAbstract>();
        
        public abstract Task<bool> Write(IDictionary<string, object> tagsValues);
    }
}
