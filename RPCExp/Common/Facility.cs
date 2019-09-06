using RPCExp.Modbus;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{
    
    public class Facility: INameDescription
    {
        char nameSeparator => Store.nameSeparator;

        public string Name { get; set; }

        public string Description { get; set; }
        
        public IDictionary<string, ConnectionSource> ConnectionsSource { get; set; }

        public IDictionary<string, DeviceAbstract> DevicesSource { get; set; }
        
    }
}
