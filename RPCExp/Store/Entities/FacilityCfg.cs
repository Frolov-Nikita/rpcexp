using RPCExp.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RPCExp.Store.Entities
{
    public class FacilityCfg : ICopyFrom, IIdentity
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(512)]
        public string AccessName { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        [MaxLength(512)]
        public string Description { get; set; }

        public ICollection<DeviceCfg> Devices { get; set; } = new List<DeviceCfg>();

        public void CopyFrom(object original)
        {
            var src = (FacilityCfg)original;
            AccessName = src.AccessName;
            Name = src.Name;
            Description = src.Description;
        }

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
