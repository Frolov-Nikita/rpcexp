
using System;
using System.Linq;
using System.Collections.Generic;
using RPCExp.Connections;
using RPCExp.AlarmLogger;
using RPCExp.TagLogger;
using RPCExp.AlarmLogger.Entities;

namespace RPCExp.Common
{
    public class Store 
    {
        public const string nameSeparator = "$";

        public Dictionary<string, Facility> Facilities { get; } = new Dictionary<string, Facility>();

        public IDictionary<string, ConnectionSourceAbstract> ConnectionsSources { get; } = new Dictionary<string, ConnectionSourceAbstract>();
        
        public IDictionary<string, AlarmCategory> AlarmCategories { get; } = new Dictionary<string, AlarmCategory>();

        public TagLogService TagLogService { get; } = new TagLogService();

        public AlarmService AlarmService { get; } = new AlarmService();
    }
}
