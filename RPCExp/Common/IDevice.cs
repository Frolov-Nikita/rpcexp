using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RPCExp.Common
{

    public interface IDevice
    {
        string Name { get; set; }
        string Description { get; set; }

        IDictionary<string, TagAbstract> Tags { get; }
                
        Task<int> Write(IDictionary<string, object> tagsValues);
    }
}
