using RPCExp.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Store.Entities
{
    public class FacilityCfg //: ClassWrapperAbstract<Facility>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
        
        public ICollection<DeviceCfg> Devices { get; set; }

        //public override Facility Unwrap()
        //{
        //    var obj = new Facility
        //    {
        //        Name = this.Name,
        //        Description = this.Description,
        //    };

        //    foreach(var ow in Devices)
        //    {
        //        var o = ow.Unwrap();
        //        obj.Devices.Add(o.Name, o);
        //    }
            
        //    return obj;
        //}

        //public override void Wrap(Facility obj)
        //{
        //    Name = obj.Name;
        //    Description = obj.Description;
        //    Devices = new List<DeviceCfg>(obj.Devices.Count);
        //    foreach (var d in obj.Devices.Values)
        //    {
        //        var dev = new DeviceCfg();
        //        dev.Wrap(d);
        //        Devices.Add(dev);
        //    }
        //}
    }
}
