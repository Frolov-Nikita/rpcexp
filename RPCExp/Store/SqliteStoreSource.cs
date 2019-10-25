using System;
using System.Collections.Generic;
using System.Text;
using RPCExp.Common;
using RPCExp.Store.Entities;

namespace RPCExp.Store
{
    public class SqliteStoreSource 
    {
        Dictionary<string, ProtocolSerializerAbstract> protorols = new Dictionary<string, ProtocolSerializerAbstract>
        {
            ["ModbusDevice"] = new ProtocolSerializerModbus(),
        };

        public Common.Store Get(string target)
        {
            return Load(target);
        }

        public Common.Store Load(string target)
        {
            var context = new StoreContext(target);
            var store = new Common.Store();

            foreach (var ow in context.Connections)
            {
                //var o = ow.Unwrap();
                //store.ConnectionsSources.Add(o.Name, o);
            }

            foreach (var ow in context.Facilities)
            {
                //var o = ow.Unwrap();
                //store.Facilities.Add(o.Name, o);
            }

            return store;
        }

        public void Save( Common.Store store, string target)
        {
            StoreContext context = new StoreContext();

            var ProtocolSerializer = protorols["ModbusDevice"];

            foreach (var f in store.Facilities.Values)
            {
                var fcfg = new FacilityCfg {
                    Name = f.Name,
                    Description = f.Description,
                };

                foreach (var d in f.Devices.Values)
                {
                    var dcfg = ProtocolSerializer.PackDevice(d);
                    fcfg.Devices.Add(dcfg);
                }
                    
                    
                context.Facilities.Add(fcfg);
            }

            context.SaveChanges();
        }
    }
}
