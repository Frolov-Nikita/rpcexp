
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

        public IDictionary<string, Facility> Facilities { get; set; } = new Dictionary<string, Facility>();

        public IDictionary<string, ConnectionSource> ConnectionsSources { get; set; } = new Dictionary<string, ConnectionSource>();


    }
}
