using RPCExp.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Store.Entities
{
    public class FacilityWrapper : ClassWrapperAbstract<Facility>
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<string> Connections { get; set; }

        public ICollection<DeviceWrapper> Devices { get; set; }

        public override Facility Unwrap()
        {
            var obj = new Facility
            {
                Name = this.Name,
                Description = this.Description,
            };

            //obj.ConnectionsSource = new Dictionary<string, ConnectionSource>();
            //foreach (var c in Connections)
            //{
            //    var cs = new ConnectionSource(c);
            //    obj.ConnectionsSource.Add(cs.Cfg, cs);
            //}

            //obj.DevicesSource = Devices.ToDictionary(d => d.Name, d => {
            //    var dev = d.Unwrap();
            //    if (obj.ConnectionsSource.TryGetValue(d.ConnectionCfg, out ConnectionSource cs))
            //        dev.Connection = cs;
            //    else
            //    {
            //        var csnew = new ConnectionSource(d.ConnectionCfg);
            //        obj.ConnectionsSource.Add(csnew.Cfg, csnew);
            //        dev.Connection = csnew;
            //    }
            //    return (DeviceAbstract)dev;
            //});

            return obj;
        }


        public override void Wrap(Facility obj)
        {
            //Name = obj.Name;
            //Description = obj.Description;
            //Devices = new List<DeviceWrapper>(obj.DevicesSource.Count);
            //foreach (var d in obj.DevicesSource.Values)
            //{
            //    var dev = new DeviceWrapper();
            //    dev.Wrap((Device)d);
            //    Devices.Add(dev);
            //}
            //Connections = obj.ConnectionsSource.Values.Select(v => v.Cfg).ToList();
        }
    }
}
