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

        public List<AlarmConfig> Configs { get; } = new List<AlarmConfig>();

        ~AlarmService()
        {
            context?.SaveChanges();
            context?.Dispose();
            context = null;
        }

        private AlarmContext Context
        {
            get
            {
                // TODO теоретически утечка памяти.
                if (context == default)
                    context = new AlarmContext();
                return context;
            }
        }

        protected override async Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            // Старт (Инициализация контекста БД алармов)
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
                cfg.AlarmInfo = storedAlarmInfo;
            }

            if (needToSave)
            { 
                await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                needToSave = false;
            }

            // Главный цикл (проверка алармов и запись)
            while (!cancellationToken.IsCancellationRequested) 
            {
                foreach(var cfg in Configs)
                {
                    if (!cfg.IsOk)
                        continue;
                    try
                    {
                        if (cfg.IsRized())
                        {
                            var alarm = new Alarm
                            {
                                TimeStamp = DateTime.Now.Ticks,
                                //alarm.AlarmInfo = cfg.AlarmInfo;
                                AlarmInfoId = cfg.AlarmInfo.Id,
#pragma warning disable CA1305 // Укажите IFormatProvider
                                Custom1 = cfg.Custom1?.GetValue().ToString(),
                                Custom2 = cfg.Custom2?.GetValue().ToString(),
                                Custom3 = cfg.Custom3?.GetValue().ToString(),
                                Custom4 = cfg.Custom4?.GetValue().ToString()
#pragma warning restore CA1305 // Укажите IFormatProvider
                            };

                            Context.Alarms.Add(alarm);
                            needToSave = true;
                        }
                    }
                    catch//(Exception ex)
                    {
                        // TODO: log this exception
                    }
                }

                try
                {
                    if (needToSave)
                    {
                        await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        context?.Dispose();
                        context = null;
                        needToSave = false;
                    }
                }catch//(Exception ex)
                {
                    //TODO: log this exception
                }
                

                await Task.Delay(100).ConfigureAwait(false);
            }

            // Завершение
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Получение списка сконфигурировных сообщений
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AlarmInfo> GetInfos()
        {
            return from cfg in Configs
                   select cfg.AlarmInfo;
        }

        /// <summary>
        /// получение списка использующихся категорий
        /// </summary>
        /// <returns></returns>
        public IQueryable<AlarmCategory> GetCategories()
        {
            return from c in Context.AlarmCategories
                   select c;
        }

        /// <summary>
        /// Доступ к архиву сообщений
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IQueryable<Alarm> GetAlarms(AlarmFilter filter = default)
        {
            var result =  from a in Context.Alarms
                   select a;

            if(filter != default)
            {
                if (filter.TBegin != long.MinValue)
                    result = result.Where(a => a.TimeStamp >= filter.TBegin);

                if (filter.TEnd != long.MaxValue)
                    result = result.Where(a => a.TimeStamp <= filter.TEnd);

                if (filter.DeviceName != default)
                    result = result.Where(a => a.AlarmInfo.DeviceName == filter.DeviceName);

                if (filter.AlarmInfoId != default)
                    result = result.Where(a => a.AlarmInfo.Id == filter.AlarmInfoId);

                if (filter.AlarmCategoriesIds?.Count() > 0)
                    result = result.Where(a => filter.AlarmCategoriesIds.Contains( a.AlarmInfo.Category.Id ));

                if (filter.FacilityAccessName != default)
                    result = result.Where(a => a.AlarmInfo.FacilityAccessName.Contains(filter.FacilityAccessName, StringComparison.OrdinalIgnoreCase));

                if (filter.Count != 0)
                    result = result.Skip(filter.Offset).Take(filter.Count);                
            }

            return result;
        }
    }

    /// <summary>
    /// Фильтр для сообщений
    /// </summary>
    public class AlarmFilter
    {
        public long TBegin { get; set; } = long.MinValue;

        public long TEnd { get; set; } = long.MaxValue;

        public IEnumerable<int> AlarmCategoriesIds { get; set; }

        public string FacilityAccessName { get; set; }

        public string DeviceName { get; set; }

        public int AlarmInfoId { get; set; }

        public int Offset { get; set; } = 0;

        public int Count { get; set; } = 0;

    }
}
