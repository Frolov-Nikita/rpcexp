using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{
    public class Facility
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<DeviceAbstract> Devices { get; set; }

    }
}
