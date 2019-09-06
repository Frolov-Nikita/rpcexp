using System.Linq;
using System.Collections.Generic;

namespace RPCExp.Common
{
    public class Store : INameDescription
    {
        public const char nameSeparator = '/';
        
        public IDictionary<string, Facility> Facilities { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }

    }
}
