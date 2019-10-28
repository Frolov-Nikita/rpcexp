
using System;
using System.Linq;
using System.Collections.Generic;
using RPCExp.Modbus;
using RPCExp.Connections;
using RPCExp.AlarmLogger.Model;

namespace RPCExp.Common
{
    public class Store 
    {
        public const char nameSeparator = '/';

        public IDictionary<string, Facility> Facilities { get; set; } = new Dictionary<string, Facility>();

        public IDictionary<string, ConnectionSource> ConnectionsSources { get; set; } = new Dictionary<string, ConnectionSource>();

        public IDictionary<string, TagsGroup> TagsGroups { get; set; } = new Dictionary<string, TagsGroup>();

        public IDictionary<string, AlarmCategory> AlarmCategories { get; set; } = new Dictionary<string, AlarmCategory>();

    }
}
