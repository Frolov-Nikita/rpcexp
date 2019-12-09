using RPCExp.AlarmLogger;
using RPCExp.Connections;
using RPCExp.TagLogger;
using System.Collections.Generic;

namespace RPCExp.Common
{
    /// <summary>
    /// Main container for services and resources
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

        /// <summary>
        /// HashSet of connection resources for entire project
        /// </summary>
        public IDictionary<string, ConnectionSourceAbstract> ConnectionsSources { get; } = new Dictionary<string, ConnectionSourceAbstract>();

        /// <summary>
        /// Tag logging service
        /// </summary>
        public TagLogService TagLogService { get; } = new TagLogService();

        /// <summary>
        /// Alarm(message) logging service
        /// </summary>
        public AlarmService AlarmService { get; } = new AlarmService();

        /// <summary>
        /// gets info about all facilities in the storage
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> GetFacilitiesInfos()
        {
            foreach (var facility in Facilities.Values)
                yield return new {
                    facility.AccessName,
                    facility.Name,
                    facility.Description,
                    Devices = facility.Devices.Keys,
                };
        }
    }
}
