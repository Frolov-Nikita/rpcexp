using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RPCExp.Common
{

    public interface IDevice
    {
        public string Name { get; set; }
        public string Description { get; set; }

        IDictionary<string, TagAbstract> Tags { get; }
                
        Task<bool> Write(IDictionary<string, object> tagsValues);
    }
}
