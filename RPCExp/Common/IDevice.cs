using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RPCExp.Common
{

    public interface IDevice: INameDescription
    {
        long BadCommWaitPeriod { get; set; }

        bool InActiveUpdate { get; set; }

        long InActiveUpdatePeriod { get; set; }

        IDictionary<string, TagsGroup> Groups { get; set; }

        IDictionary<string, TagAbstract> Tags { get; }

        Task<int> Write(IDictionary<string, object> tagsValues);
    }
}
