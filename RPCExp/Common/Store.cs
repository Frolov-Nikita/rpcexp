
using System;
using System.Linq;
using System.Collections.Generic;
using RPCExp.Modbus;
using RPCExp.Connections;

namespace RPCExp.Common
{
    public class Store 
    {
        public const char nameSeparator = '/';
        
        public IDictionary<string, Facility> Facilities { get; set; }
        
        public IDictionary<string, IConnectionSource> ConnectionsSources { get; set; }


    }
}
