using RPCExp.AlarmLogger;
using RPCExp.Connections;
using RPCExp.TagLogger;
using System.Collections.Generic;

namespace RPCExp.Common
{
    public class Store
    {
        public const string nameSeparator = "$";

        public Dictionary<string, Facility> Facilities { get; } = new Dictionary<string, Facility>();

        public IDictionary<string, ConnectionSourceAbstract> ConnectionsSources { get; } = new Dictionary<string, ConnectionSourceAbstract>();

        public TagLogService TagLogService { get; } = new TagLogService();

        public AlarmService AlarmService { get; } = new AlarmService();
    }
}
