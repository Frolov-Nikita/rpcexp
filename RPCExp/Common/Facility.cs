
using System;

using System.Collections.Generic;


namespace RPCExp.Common
{
    public class Facility : INameDescription
    {
        public int Id { get; set; }

        public string AccessName { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public IDictionary<string, DeviceAbstract> Devices { get; } = new Dictionary<string, DeviceAbstract>();

    }
}
