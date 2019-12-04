using RPCExp.AlarmLogger;
using RPCExp.Connections;
using RPCExp.TagLogger;
using System.Collections.Generic;

namespace RPCExp.Common
{
    /// <summary>
    /// main container for services and resources
    /// </summary>
    public class Store
    {
        /// <summary>
        /// Separator to build tree nodes from access names
        /// </summary>
        public const string nameSeparator = "$";

        /// <summary>
        /// HashSet of automation objects, hashed by AccesNames
        /// </summary>
        public Dictionary<string, Facility> Facilities { get; } = new Dictionary<string, Facility>();


        public IDictionary<string, ConnectionSourceAbstract> ConnectionsSources { get; } = new Dictionary<string, ConnectionSourceAbstract>();

        public TagLogService TagLogService { get; } = new TagLogService();

        public AlarmService AlarmService { get; } = new AlarmService();
    }
}
