﻿using Microsoft.EntityFrameworkCore;
using RPCExp.Common;
using RPCExp.DbStore.Entities;
using RPCExp.DbStore.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPCExp.DbStore
{
    /// <summary>
    /// Класс сохранения конфигурации в БД и восстановления конфигурации из БД
    /// </summary>
    public class SqliteStoreSource : IDisposable
    {
        private Dictionary<string, ProtocolSerializerAbstract> protorolSerializers = new Dictionary<string, ProtocolSerializerAbstract>();
        private Dictionary<string, IConnectionSourceSerializer> connectionSerializers = new Dictionary<string, IConnectionSourceSerializer>();

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

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Same as Load
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Store Get(string target)
        {
            return Load(target);
        }

        /// <summary>
        /// Loads and restores main storage
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Store Load(string target)
        {
            var context = new StoreContext(target);

            var store = new Store();
            var scales = new Dictionary<int, Scale>();

            foreach(var scaleCfg in context.Scales)
                scales.Add(
                    scaleCfg.Id, 
                    new Scale {
                        Name = scaleCfg.Name,
                        Description = scaleCfg.Description,
                        Units = scaleCfg.Units,
                        DevMax = scaleCfg.DevMax,
                        DevMin = scaleCfg.DevMin,
                        Max = scaleCfg.Max,
                        Min = scaleCfg.Min,
                    }
                );
            

            foreach (var cfg in context.Connections)
            {
                var connectionSource = connectionSerializers[cfg.ClassName].Unpack(cfg);
                store.ConnectionsSources.Add(cfg.Name, connectionSource);
            }

            var storedDeviceToTemplates = context.DeviceToTemplates
                .Include(e => e.Device)
                .Include(e => e.Template)
                    .ThenInclude(e => e.Tags)
                    .ThenInclude((TagCfg e) => e.TagsToTagsGroups)
                    .ThenInclude((TagsToTagsGroups e) => e.TagsGroupCfg)
                .Include(e => e.Template)
                    .ThenInclude(e => e.Alarms)
                    .ThenInclude((AlarmCfg e) => e.Category)
                .Include(e => e.Template)
                    .ThenInclude(e => e.Archives)
                .ToList();

            var facilityConfigs = context.Facilities.Include(e => e.Devices).ToList();
            foreach (var facilityCfg in facilityConfigs)
            {
                var facility = new Facility
                {
                    AccessName = facilityCfg.AccessName,
                    Name = facilityCfg.Name,
                    Description = facilityCfg.Description,
                    Id = facilityCfg.Id,
                };


                foreach (var deviceCfg in facilityCfg.Devices)
                {
                    var protocolSerializer = protorolSerializers[deviceCfg.ClassName];
                    var device = protocolSerializer.UnpackDevice(deviceCfg, store);

                    // Обработка шаблона (переделать этот код когда в EF Core будет реализован релейшн many2many)
                    foreach (var deviceToTemplate in storedDeviceToTemplates.Where(e => e.DeviceId == deviceCfg.Id))
                    {
                        var template = deviceToTemplate.Template;
                        // Распакуем тэги
                        foreach (var tagCfg in template.Tags)
                        {
                            var tag = protocolSerializer.UnpackTag(tagCfg);
                            tag.TemplateId = template.Id;
                            if(scales.ContainsKey(tagCfg.ScaleId))
                                tag.Scale = scales[tagCfg.ScaleId];

                            // добавим/восстановим/создадим группы тэгов

                            foreach (var tttg in tagCfg.TagsToTagsGroups)
                            {
                                var tagGroupCfg = tttg.TagsGroupCfg;
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
                            if (tagCfg.ArchiveCfg != default)
                            {
                                var tagLogConfig = new TagLogger.TagLogConfig(tag)
                                {
                                    Hyst = tagCfg.ArchiveCfg.Hyst,
                                    PeriodMaxSec = tagCfg.ArchiveCfg.PeriodMaxSec,
                                    PeriodMinSec = tagCfg.ArchiveCfg.PeriodMinSec,
                                    TagLogInfo = new TagLogger.Entities.TagLogInfo
                                    {
                                        DeviceName = deviceCfg.Name,
                                        FacilityAccessName = facilityCfg.AccessName,
                                        TagName = tagCfg.Name,
                                    }
                                };

                                store.TagLogService.Configs.Add(tagLogConfig);
                            }

                            device.Tags.AddByName(tag);
                        }

                        // Распакуем алармы
                        foreach (var alarmCfg in template.Alarms)
                        {
                            var alarmInfo = new AlarmLogger.Entities.AlarmInfo
                            {
                                Category = alarmCfg.Category,
                                Name = alarmCfg.Name,
                                Condition = alarmCfg.Condition,
                                Description = alarmCfg.Description,
                                DeviceName = device.Name,
                                FacilityAccessName = facility.AccessName,
                                TemplateTxt = alarmCfg.TemplateTxt,
                            };
                            var alarmConfig = AlarmLogger.AlarmConfig.From(alarmCfg, device.Tags.Values, alarmInfo);
                            store.AlarmService.Configs.Add(alarmConfig);
                        }
                    }

                    facility.Devices.AddByName(device);
                }

                store.Facilities.Add(facility.AccessName, facility);
            }
            context.Dispose();
            context = null;
            return store;
        } // Load();

        /// <summary>
        /// Saves storage into DB.
        /// it doesn't work properly, cause of templates,
        /// but it makes good initialization for new db.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="target"></param>
        public void Save(Store store, string target)
        {
            if (store is null)
                throw new ArgumentNullException(nameof(store));

            var context = new StoreContext(target);

            var protocolSerializer = protorolSerializers["Modbus"];

            foreach (var cs in store.ConnectionsSources.Values)
            {
                var csConfig = connectionSerializers[cs.ClassName].Pack(cs);
                var stored = context.Connections.GetOrCreate(o => o.Name == csConfig.Name);
                stored.CopyFrom(csConfig);
            }

            foreach (var facility in store.Facilities.Values)
            {
                var facilityCfg = new FacilityCfg
                {
                    Name = facility.Name,
                    AccessName = facility.AccessName,
                    Description = facility.Description,
                    Id = facility.Id,
                };

                var storedFacility = context.Facilities.GetOrCreate(f => f.Name == facilityCfg.Name);
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
            context.Dispose();
        }
    }
}
