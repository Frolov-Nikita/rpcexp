using RPCExp.AlarmLogger.Model;
using RPCExp.Common;
using System;
using System.Linq;
using RPCExp.Store.Entities;
using System.Collections.Generic;

namespace RPCExp.Store.Serializers
{

    /// <summary>
    /// Класс преобразует сущности из БД в объекты программы и обратно.
    /// </summary>
    /// <typeparam name="T">Сласс устройства реализующего протокол</typeparam>
    internal abstract class ProtocolSerializerAbstract
    {
        public ProtocolSerializerAbstract()
        {
        }

        public abstract string ClassName { get; } 

        public Common.Store Store { get; }

        public DeviceAbstract UnpackDevice(DeviceCfg config, Common.Store store)
        {
            DeviceAbstract device = UnpackDeviceSpecific(config.Custom);

            device.Name = config.Name;
            device.Description = config.Description;
            device.BadCommWaitPeriod = config.BadCommWaitPeriod;
            device.InActiveUpdate = config.InActiveUpdate;
            device.InActiveUpdatePeriod = config.InActiveUpdatePeriod;

            device.ConnectionSource = store.ConnectionsSources.Values.FirstOrDefault(c => c.Name == config.ConnectionSourceCfg.Name);

            //foreach (var d2t in config.DeviceToTemplates)
            //{
            //    var template = d2t.Template;
            //    foreach(var tagCfg in template.Tags)
            //    {
            //        var tag = UnpackTag(tagCfg);
            //        device.Tags.Add(tag.Name, tag);
            //    }

            //    foreach (var alarmCfg in template.Alarms)
            //    {
            //        var alarm = UnpackAlarm(alarmCfg);
            //        device.AlarmsConfig.Add(alarm);
            //    }

            //    foreach (var archCfg in template.Archives)
            //    {
            //        // TODO: распаковка архивных тегов
            //    }

            //}

            return device;
        }

        public DeviceCfg PackDevice(DeviceAbstract device, StoreContext context)
        {
            var config = context.Devices.GetOrCreate(d => d.Name == device.Name);

            config.ClassName = this.ClassName;
            config.Name = device.Name;
            config.Description = device.Description;
            config.BadCommWaitPeriod = device.BadCommWaitPeriod;
            config.InActiveUpdate = device.InActiveUpdate;
            config.InActiveUpdatePeriod = device.InActiveUpdatePeriod;

            config.ConnectionSourceCfg = context.Connections.GetOrCreate(c => c.Name == device.ConnectionSource.Name);
            
            config.Custom = PackDeviceSpecific(device);
            
            foreach (var tag in device.Tags.Values)
            {
                var tagCfg = PackTag(tag, context);

                var storedTemplate = context.Templates.GetOrCreate(t => t.Id == tag.TemplateId);
                storedTemplate.Id = tag.TemplateId; //если он создался заново

                var dev2Templ = config.DeviceToTemplates.FirstOrDefault(d2t => d2t.Template.Id == tag.TemplateId);

                if(dev2Templ == default)
                {
                    dev2Templ = context.DeviceToTemplates.GetOrCreate(d2t => d2t.TemplateId == tag.TemplateId && d2t.DeviceId == config.Id);
                    dev2Templ.Device = config;
                    dev2Templ.Template = storedTemplate;
                    config.DeviceToTemplates.Add(dev2Templ);
                }

                dev2Templ.Template.Tags.Add(tagCfg);

                //var storedDeviceToTemplate = context.DeviceToTemplates.GetOrCreate(d2t => d2t.TemplateId == tag.TemplateId && d2t.DeviceId == config.Id);

                //storedDeviceToTemplate.Template = storedTemplate;
                //storedDeviceToTemplate.TemplateId = storedTemplate.Id;
                //storedDeviceToTemplate.Device = config;
                //storedDeviceToTemplate.DeviceId = config.Id;

                //if (!storedTemplate.Tags.Contains(tagCfg))
                //    storedTemplate.Tags.Add(tagCfg);

                //if (!config.DeviceToTemplates.Contains(storedDeviceToTemplate))
                //    config.DeviceToTemplates.Add(storedDeviceToTemplate);

            }
            return config;
        }

        public TagAbstract UnpackTag(TagCfg config)
        {
            var t = UnpackTagSpecific(config.Custom);

            t.Name = config.Name;
            t.DisplayName = config.DisplayName;
            t.Description = config.Description;
            t.Units = config.Units;
            t.Format = config.Format;
            t.Access = config.Access;
            t.ValueType = config.ValueType;
            t.Scale = new Scale
            {
                DevMax = config.ScaleDevMax,
                DevMin = config.ScaleDevMin,
                Max = config.ScaleMax,
                Min = config.ScaleMin,
            };

            return t;
        }

        public TagCfg PackTag(TagAbstract tag, StoreContext context)
        {
            var config = new TagCfg
            {
                ClassName = this.ClassName,
                Name = tag.Name,
                DisplayName = tag.DisplayName,
                Description = tag.Description,
                Units = tag.Units,
                Format = tag.Format,
                Access = tag.Access,
                ValueType = tag.ValueType,
                ScaleDevMax = tag.Scale?.DevMax ?? Int16.MaxValue,
                ScaleDevMin = tag.Scale?.DevMin ?? Int16.MinValue,
                ScaleMax = tag.Scale?.Max ?? Int16.MaxValue,
                ScaleMin = tag.Scale?.Min ?? Int16.MinValue,
                Custom = PackTagSpecific(tag),
                TagsToTagsGroups = new List<TagsToTagsGroups>(),
            };

            foreach (var tagsGroup in tag.Groups.Values)
            {
                var storedGroup = context.TagsGroups.GetOrCreate(tg => tg.Name == tagsGroup.Name);
                storedGroup.Name = tagsGroup.Name;
                storedGroup.Min = tagsGroup.Min;
                storedGroup.Description = tagsGroup.Description;

                var ttg = config.TagsToTagsGroups.FirstOrDefault(o => o.TagsGroupCfg.Name == storedGroup.Name);
                if(ttg == default)
                {
                    ttg = context.TagsToTagsGroups.GetOrCreate(o => o.TagsGroupCfg.Name == storedGroup.Name && o.TagCfg.Id == config.Id);
                    ttg.TagCfg = config;
                    ttg.TagsGroupCfg = storedGroup;
                }
                config.TagsToTagsGroups.Add(ttg);
            }                

            return config;
        }

        protected virtual AlarmConfig UnpackAlarm(AlarmCfg config)
        {
            throw new NotImplementedException();
        }

        protected virtual AlarmCfg PackAlarm(AlarmConfig alarm)
        {
            throw new NotImplementedException();
        }

        protected abstract string PackDeviceSpecific(DeviceAbstract device);

        protected abstract DeviceAbstract UnpackDeviceSpecific(string custom);

        protected abstract TagAbstract UnpackTagSpecific(string custom);

        protected abstract string PackTagSpecific(TagAbstract tag);


    }
}