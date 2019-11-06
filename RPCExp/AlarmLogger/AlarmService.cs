using RPCExp.AlarmLogger.Entities;
using RPCExp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RPCExp.AlarmLogger
{
    public class AlarmService : ServiceAbstract
    {

        AlarmContext context;

        public List<AlarmConfig> Configs { get; set; }

        ~AlarmService()
        {
            context?.SaveChanges();
        }

        private AlarmContext Context
        {
            get
            {
                if (context == default)
                    context = new AlarmContext();
                return context;
            }
        }

        protected override async Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            // Старт (Инициализация контекста БД алармов)
            // TODO: !! Добавить тегам группу опроса!
            bool needToSave = false;
            foreach (var cfg in Configs)
            {
                //синхронизируем категорию
                var storedCategory = Context.AlarmCategories.FirstOrDefault(e => 
                    e.Name == cfg.AlarmInfo.Category.Name &&
                    e.Style == cfg.AlarmInfo.Category.Style
                    );
                if(storedCategory == default)
                {
                    storedCategory = new AlarmCategory
                    {
                        Name = cfg.AlarmInfo.Category.Name,
                        Style = cfg.AlarmInfo.Category.Style,
                    };
                    Context.AlarmCategories.Add(storedCategory);
                    needToSave = true;
                }
                cfg.AlarmInfo.Category = storedCategory;

                //синхронизируем алармИнфо
                var storedAlarmInfo = Context.AlarmsInfo.FirstOrDefault(e =>
                    e.Category == cfg.AlarmInfo.Category &&
                    e.FacilityAccessName == cfg.AlarmInfo.FacilityAccessName &&
                    e.DeviceName == cfg.AlarmInfo.DeviceName &&
                    e.Name == cfg.AlarmInfo.Name &&
                    e.Description == cfg.AlarmInfo.Description &&
                    e.Condition == cfg.AlarmInfo.Condition &&
                    e.TemplateTxt == cfg.AlarmInfo.TemplateTxt);

                if(storedAlarmInfo == default)
                {
                    storedAlarmInfo = new AlarmInfo
                    {
                        Category = cfg.AlarmInfo.Category,
                        FacilityAccessName = cfg.AlarmInfo.FacilityAccessName,
                        DeviceName = cfg.AlarmInfo.DeviceName,
                        Name = cfg.AlarmInfo.Name,
                        Description = cfg.AlarmInfo.Description,
                        Condition = cfg.AlarmInfo.Condition,
                        TemplateTxt = cfg.AlarmInfo.TemplateTxt,
                    };

                    Context.AlarmsInfo.Add(storedAlarmInfo);                    
                    needToSave = true;
                }
            }

            if (needToSave)
            { 
                await Context.SaveChangesAsync(cancellationToken);
                needToSave = false;
            }

            // Главный цикл (проверка алармов и запись)
            while (!cancellationToken.IsCancellationRequested) 
            {
                
                foreach(var cfg in Configs)
                {
                    //try
                    //{
                    if (cfg.IsRized())
                    {
                        var alarm = new Alarm();
                        alarm.TimeStamp = DateTime.Now.Ticks;
                            
                        alarm.CustomTag1 = cfg.CustomTag1?.GetValue().ToString();
                        alarm.CustomTag2 = cfg.CustomTag2?.GetValue().ToString();
                        alarm.CustomTag3 = cfg.CustomTag3?.GetValue().ToString();
                        alarm.CustomTag4 = cfg.CustomTag4?.GetValue().ToString();

                        Context.Alarms.Add(alarm);
                        needToSave = true;
                    }
                    //}
                }

                if (needToSave)
                {
                    await Context.SaveChangesAsync(cancellationToken);
                    needToSave = false;
                }

                await Task.Delay(100);
            }

            // Завершение
            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}
