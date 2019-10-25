using RPCExp.Modbus;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPCExp.Common
{
    
    public class Facility: INameDescription
    {
        [Key]
        [MaxLength(64)]
        public string Name { get; set; }


        [MaxLength(512)]
        public string Description { get; set; }

        //TODO: переместить в store
        //[NotMapped]
        //public IDictionary<string, ConnectionSource> ConnectionsSource { get; set; }


        [NotMapped]
        public IDictionary<string, DeviceAbstract> Devices { get; set; }

        public Dictionary<string, ConnectionSource> ConnectionsSource { get; internal set; }
    }
}
