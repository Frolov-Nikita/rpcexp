using System.Collections.Generic;
using System.Linq;
using RPCExp.Common;

namespace RPCExp.Modbus.Factory
{
    public class FacilityCfgWrapper : IClassWrapper<Facility>
    {
        public string ClassName => nameof(Facility);

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<string> Connections { get; set; }

        public ICollection<DeviceCfgWrapper> Devices { get; set; }

        public Facility Unwrap()
        {
            var obj = new Facility
            {
                Name = this.Name,
                Description = this.Description,
            };

            obj.ConnectionsSource = new Dictionary<string, ConnectionSource>();
            foreach (var c in Connections)
            {
                var cs = new ConnectionSource(c);
                obj.ConnectionsSource.Add(cs.Cfg, cs);
            }

            obj.Devices = Devices.ToDictionary(d=>d.Name, d=> {
                var dev = d.Unwrap();
                if (obj.ConnectionsSource.TryGetValue(d.ConnectionCfg, out ConnectionSource cs))
                    dev.Connection = cs;
                else
                {
                    var csnew = new ConnectionSource(d.ConnectionCfg);
                    obj.ConnectionsSource.Add(csnew.Cfg, csnew);
                    dev.Connection = csnew;
                }
                return (DeviceAbstract)dev;
            });

            return obj;
        }

        public void Wrap(Facility obj)
        {
            Name = obj.Name;
            Description = obj.Description;
            Devices = new List<DeviceCfgWrapper>(obj.Devices.Count);
            foreach (var d in obj.Devices.Values)
            {
                var dev = new DeviceCfgWrapper();
                dev.Wrap((ModbusDevice)d);
                Devices.Add(dev);
            }
            Connections = obj.ConnectionsSource.Values.Select(v=>v.Cfg).ToList();
        }
    }

}
