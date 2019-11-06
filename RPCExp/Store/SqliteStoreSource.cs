using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using RPCExp.Common;
using RPCExp.Connections;
using RPCExp.Store.Entities;
using RPCExp.Store.Serializers;
using Microsoft.EntityFrameworkCore;

namespace RPCExp.Store
{
    /// <summary>
    /// Класс сохранения конфигурации в БД и восстановления конфигурации из БД
    /// </summary>
    public class SqliteStoreSource 
    {
        Dictionary<string, ProtocolSerializerAbstract> protorolSerializers = new Dictionary<string, ProtocolSerializerAbstract>();
        Dictionary<string, IConnectionSourceSerializer> connectionSerializers = new Dictionary<string, IConnectionSourceSerializer>();

        public SqliteStoreSource()
        {
            var protocolSerializerModbus = new ProtocolSerializerModbus();
            protorolSerializers.Add(protocolSerializerModbus.ClassName, protocolSerializerModbus);

            var tcpConnectionSourceSerializer = new TcpConnectionSourceSerializer();
            var udpConnectionSourceSerializer = new UdpConnectionSourceSerializer();
            var serialConnectionSourceSerializer = new SerialConnectionSourceSerializer();

            connectionSerializers.Add(tcpConnectionSourceSerializer.ClassName, tcpConnectionSourceSerializer);
            connectionSerializers.Add(udpConnectionSourceSerializer.ClassName, udpConnectionSourceSerializer);
            connectionSerializers.Add(serialConnectionSourceSerializer.ClassName, serialConnectionSourceSerializer);
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
                var connectionSource = connectionSerializers[cfg.ClassName].Unpack(cfg);                
                store.ConnectionsSources.Add(cfg.Name, connectionSource);
            }

            var storedDeviceToTemplates = context.DeviceToTemplates
                .Include(dtt => dtt.Device)
                .Include(dtt => dtt.Template)
                    .ThenInclude(t => t.Tags)
                .Include(o => o.Template)
                    .ThenInclude(t => t.Alarms)
                .Include(o => o.Template)
                    .ThenInclude(t => t.Archives);

            var storedTagsToTagsGroup = context.TagsToTagsGroups
                .Include(ttg => ttg.TagCfg)
                .Include(ttg => ttg.TagsGroupCfg);
            
            foreach (var facilityCfg in context.Facilities.Include(f => f.Devices))
            {
                var facility = new Facility
                {
                    AccessName = facilityCfg.AccessName,
                    Name = facilityCfg.Name,
                    Description = facilityCfg.Description,
                    Id = facilityCfg.Id,
                };

                foreach(var deviceCfg in facilityCfg.Devices)
                {

                    var protocolSerializer = protorolSerializers[deviceCfg.ClassName];
                    var device = protocolSerializer.UnpackDevice(deviceCfg, store);

                    // Обрвботка шаблона
                    foreach (var deviceToTemplate in storedDeviceToTemplates.Where(e => e.DeviceId == deviceCfg.Id))
                    {
                        var template = deviceToTemplate.Template;
                        // Распакуем тэги
                        foreach (var tagCfg in template.Tags)
                        {
                            var tag = protocolSerializer.UnpackTag(tagCfg);

                            // добавим/восстановим/создадим группы тэгов
                            foreach(var tagToTagsGroupCfg in storedTagsToTagsGroup.Where(e => e.TagId == tagCfg.Id))
                            {
                                var tagGroupCfg = tagToTagsGroupCfg.TagsGroupCfg;
                                TagsGroup tagGroup;
                                if (device.Groups.ContainsKey(tagGroupCfg.Name))
                                    tagGroup = device.Groups[tagGroupCfg.Name];
                                else
                                {
                                    tagGroup = new TagsGroup
                                    {
                                        Name = tagGroupCfg.Name,
                                        Description = tagGroupCfg.Description,
                                        Min = tagGroupCfg.Min,
                                    };
                                    device.Groups.AddByName(tagGroup);
                                }
                                tag.Groups.AddByName(tagGroup);
                            }
                            
                            // добавим тэг в сервис архива
                            if(tagCfg.ArchiveCfg != default)
                            {
                                var tlc = new TagLogger.TagLogConfig(tag)
                                {
                                    HystProc = tagCfg.ArchiveCfg.HystProc,
                                    PeriodMaxSec = tagCfg.ArchiveCfg.PeriodMaxSec,
                                    PeriodMinSec = tagCfg.ArchiveCfg.PeriodMinSec,
                                    TagLogInfo = new TagLogger.Entities.TagLogInfo { 
                                        DeviceName = deviceCfg.Name,
                                        FacilityAccessName = facilityCfg.Name,
                                        TagName = tagCfg.Name,
                                    }
                                };

                                store.TagLogManager.Configs.Add(tlc);
                            }

                            device.Tags.AddByName(tag);
                        }

                        // Распакуем алармы
                        foreach (var alarmCfg in template.Alarms)
                        {
                            alarmCfg.
                        }

                    }

                    facility.Devices.Add(device.Name, device);
                }

                store.Facilities.Add(facility.Name, facility);
            }

            return store;
        }

        public void Save( Common.Store store, string target)
        {
            var context = new StoreContext(target);

            var protocolSerializer = protorolSerializers["Modbus"];

            foreach (var cs in store.ConnectionsSources.Values)
            {
                var csConfig = connectionSerializers[cs.ClassName].Pack(cs);
                var stored = context.Connections.GetOrCreate( o=>o.Name == csConfig.Name);
                stored.CopyFrom(csConfig);
            }

            foreach (var facility in store.Facilities.Values)
            {
                var facilityCfg = new FacilityCfg {
                    Name = facility.Name,
                    AccessName = facility.AccessName,
                    Description = facility.Description,
                    Id = facility.Id,
                };

                var storedFacility = context.Facilities.GetOrCreate( f => f.Name == facilityCfg.Name);
                storedFacility.CopyFrom(facilityCfg);
                
                foreach (var device in facility.Devices.Values)
                {
                    var deviceCfg = protocolSerializer.PackDevice(device, context);
                    var storedDevice = context.Devices.GetOrCreate(d => d.Name == device.Name);
                    storedDevice.CopyFrom(deviceCfg);

                    storedFacility.Devices.Add(storedDevice);
                }
            }

            context.SaveChanges();
        }
    }
}
