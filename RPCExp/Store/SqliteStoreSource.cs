using System;
using System.Collections.Generic;
using System.Text;
using RPCExp.Common;
using RPCExp.Store.Entities;

namespace RPCExp.Store
{
    public class SqliteStoreSource 
    {

        StoreContext context = new StoreContext();

        Dictionary<string, ProtocolSerializerAbstract> protorols = new Dictionary<string, ProtocolSerializerAbstract>();

        public SqliteStoreSource()
        {
            var ProtocolSerializerModbus = new ProtocolSerializerModbus();
            protorols.Add(ProtocolSerializerModbus.ClassName, ProtocolSerializerModbus);
        }

        public Common.Store Get(string target)
        {
            return Load(target);
        }

        public Common.Store Load(string target)
        {
            var context = new StoreContext(target);
            var store = new Common.Store();

            foreach (var cfg in context.Connections)
            {
                var connectionSource = new Modbus.ConnectionSource()
                {
                    Cfg = cfg.Cfg,
                    Name = cfg.Name,
                    Description = cfg.Description,
                    //Physic
                };

                store.ConnectionsSources.Add(connectionSource.Name, connectionSource);
            }

            foreach (var cfg in context.TagsGroups)
            {
                var tagsGroup = new TagsGroup(cfg);
                store.TagsGroups.Add(tagsGroup.Name, tagsGroup);
            }

            foreach (var cfg in context.Facilities)
            {
                var facility = new Facility
                {
                    Name = cfg.Name,
                    Description = cfg.Description,
                };

                foreach(var dcfg in cfg.Devices)
                {
                    var protocolSerializer = protorols[dcfg.ClassName];
                    var dev = protocolSerializer.UnpackDevice(dcfg, store);
                    facility.Devices.Add(dev.Name, dev);
                }

                store.Facilities.Add(facility.Name, facility);
            }

            return store;
        }

        public void Save( Common.Store store, string target)
        {
            var protocolSerializer = protorols["Modbus"];

            // TODO: Save connections
            foreach (var cs in store.ConnectionsSources.Values)
            {
                var c = new ConnectionSourceCfg
                {
                    Cfg = cs.Cfg,
                    Name = cs.Name,
                    Description = cs.Description
                };

                var stored = context.Connections.GetOrCreate(c, o=>o.Name == c.Name);
            }

            foreach (var f in store.Facilities.Values)
            {
                var fcfg = new FacilityCfg {
                    Name = f.Name,
                    Description = f.Description,
                };

                foreach (var d in f.Devices.Values)
                {
                    var dcfg = protocolSerializer.PackDevice(d, context);

                    fcfg.Devices.Add(dcfg);
                }
                  
                context.Facilities.Add(fcfg);
            }

            context.SaveChanges();
        }
    }
}
