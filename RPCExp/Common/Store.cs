
using System;
using System.Linq;
using System.Collections.Generic;
using RPCExp.Modbus;

namespace RPCExp.Common
{
    public class Store : INameDescription
    {
        public const char nameSeparator = '/';
        
        public IDictionary<string, Facility> Facilities { get; set; }
        
        public IDictionary<string, ConnectionSource> ConnectionsSource { get; set; }

        public string Name { get; set; }
        
        public string Description { get; set; }

    }
}
