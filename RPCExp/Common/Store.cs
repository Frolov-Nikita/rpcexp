
using System;
using System.Linq;
using System.Collections.Generic;
using RPCExp.Connections;
using RPCExp.AlarmLogger.Model;

namespace RPCExp.Common
{
    public class Store 
    {
        public const string nameSeparator = "$";

        public IDictionary<string, Facility> Facilities { get; set; } = new Dictionary<string, Facility>();

        public IDictionary<string, ConnectionSourceAbstract> ConnectionsSources { get; set; } = new Dictionary<string, ConnectionSourceAbstract>();
        
        public IDictionary<string, AlarmCategory> AlarmCategories { get; set; } = new Dictionary<string, AlarmCategory>();

        public object GetNames()
        {
            return null;
        }

    }
}
