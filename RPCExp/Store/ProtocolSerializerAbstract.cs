using RPCExp.AlarmLogger.Model;
using RPCExp.Common;
using System;
using System.Linq;
using RPCExp.Store.Entities;
using System.Collections.Generic;

namespace RPCExp.Store
{

    /// <summary>
    /// Класс преобразует сущности из БД в объекты программы и обратно.
    /// </summary>
    /// <typeparam name="T">Сласс устройства реализующего протокол</typeparam>
    public abstract class ProtocolSerializerAbstract
    {
        public ProtocolSerializerAbstract()
        {
            
        }

        public abstract string ClassName { get; }

        public Common.Store Store { get; }

        public DeviceAbstract UnpackDevice(DeviceCfg config)
        {
            DeviceAbstract device = UnpackDeviceSpecific(config.Custom);

            device.Name = config.Name;
            device.Description = config.Description;
            device.BadCommWaitPeriod = config.BadCommWaitPeriod;
            device.InActiveUpdate = config.InActiveUpdate;
            device.InActiveUpdatePeriod = config.InActiveUpdatePeriod;

            // TODO: device.StreamSource = ??

            foreach (var template in config.Templates)
            {
                foreach(var tagCfg in template.Tags)
                {
                    var tag = UnpackTag(tagCfg);
                    device.Tags.Add(tag.Name, tag);
                }

                foreach (var alarmCfg in template.Alarms)
                {
                    var alarm = UnpackAlarm(alarmCfg);
                    device.AlarmsConfig.Add(alarm);
                }

                foreach (var archCfg in template.Archives)
                {
                    // TODO: распаковка архивных тегов
                }

            }

            return device;
        }

        public DeviceCfg PackDevice(DeviceAbstract device)
        {
            var config = new DeviceCfg
            {
                ClassName = "ModbusDevice", //TODO: переделать
                Name = device.Name,
                Description = device.Description,
                BadCommWaitPeriod = device.BadCommWaitPeriod,
                InActiveUpdate = device.InActiveUpdate,
                InActiveUpdatePeriod = device.InActiveUpdatePeriod
            };

            config.Custom = PackDeviceSpecific(device);

            var tags = new List<TagCfg>(device.Tags.Count);
            
            foreach (var t in device.Tags.Values)
            {
                var tcfg = PackTag(t);
                
                tags.Add(tcfg);
                
                if (!config.Templates.Exists(tem =>tem.Id == t.TemplateId))
                    config.Templates.Add(new Template { Id = t.TemplateId });

                var template = config.Templates.First(tem => tem.Id == t.TemplateId);

                template.Tags.Add(tcfg);
            }

            //throw new NotImplementedException();// TODO: Не совсем понятно, как теги завернуть обратно в шаблоны.
            return config;
        }

        protected TagAbstract UnpackTag(TagCfg config)
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

        protected TagCfg PackTag(TagAbstract tag)
        {
            var config = new TagCfg
            {
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
                
            };
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