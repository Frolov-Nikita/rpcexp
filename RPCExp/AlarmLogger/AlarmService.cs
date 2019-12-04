using Microsoft.EntityFrameworkCore;
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
        private const int baseCapacityOfTmpList = 32; // Начальная емкость промежуточного хранилища

        private const int minWaitTimeMs = 50; // Минимальное время ожидания, мсек

        public TimeSpan MinMaintainPeriod { get; set; } = TimeSpan.FromSeconds(10);

        private DateTime nextMaintain = DateTime.Now;

        public TimeSpan CheckPeriod { get; set; } = TimeSpan.FromMilliseconds(500);

        public TimeSpan SavePeriod { get; set; } = TimeSpan.FromSeconds(10);

        public long StoreItemsCount { get; set; } = 10_000_000;

        private long DeltaRecordsCount => 1 + StoreItemsCount * 5 / 100;

        public string FileName { get; set; } = "alarmLog.sqlite3";

        private readonly List<AlarmCategory> localCategories = new List<AlarmCategory>(4);

        /// <summary>
        /// Список сконфигурированных аварий.
        /// </summary>
        public List<AlarmConfig> Configs { get; } = new List<AlarmConfig>();

        /// <summary>
        /// Синхронизация/инициализация категорий и AlarmInfo
        /// </summary>
        private async Task InnitDB(CancellationToken cancellationToken)
        {
            var context = new AlarmContext(FileName);

            var storedCategories = await context.AlarmCategories.ToListAsync(cancellationToken).ConfigureAwait(false);
            var storedInfos = await context.AlarmsInfo.ToListAsync(cancellationToken).ConfigureAwait(false);

            foreach (var cfg in Configs)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                //синхронизируем категорию в 2 хранилища
                var storedCategory = storedCategories.FirstOrDefault(e =>
                    e.Name == cfg.AlarmInfo.Category.Name &&
                    e.Style == cfg.AlarmInfo.Category.Style
                    );

                if (storedCategory == default)
                {
                    storedCategory = new AlarmCategory
                    {
                        Name = cfg.AlarmInfo.Category.Name,
                        Style = cfg.AlarmInfo.Category.Style,
                    };
                    context.AlarmCategories.Add(storedCategory);
                    storedCategories.Add(storedCategory);
                }

                cfg.AlarmInfo.Category = storedCategory;

                if (!localCategories.Contains(storedCategory))
                    localCategories.Add(storedCategory);

                //синхронизируем алармИнфо
                var storedAlarmInfo = storedInfos.FirstOrDefault(e =>
                    e.Category == cfg.AlarmInfo.Category &&
                    e.FacilityAccessName == cfg.AlarmInfo.FacilityAccessName &&
                    e.DeviceName == cfg.AlarmInfo.DeviceName &&
                    e.Name == cfg.AlarmInfo.Name &&
                    e.Description == cfg.AlarmInfo.Description &&
                    e.Condition == cfg.AlarmInfo.Condition &&
                    e.TemplateTxt == cfg.AlarmInfo.TemplateTxt);

                if (storedAlarmInfo == default)
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

                    context.AlarmsInfo.Add(storedAlarmInfo);
                    storedInfos.Add(storedAlarmInfo);
                }

                cfg.AlarmInfo = storedAlarmInfo;
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            context.Dispose();
        }

        private async Task SaveAsync(Queue<Alarm> cache, CancellationToken cancellationToken)
        {
            if ((cache?.Count ?? 0) == 0)
                return;

            var context = new AlarmContext(FileName);

            /* // Код как оно должно работать
            context.Alarms.AddRange(cache);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            */

            // ########## Начало костыля
            // TODO: при новых версиях EF Core (> 3.0.1) пробовать убрать этот костыль
            const int maxItemsInInsert = 128;

            static string nullIfNull(string val) => string.IsNullOrEmpty(val) ? "null" : val;

            while (cache.Count > 0)
            {
                var len = cache.Count > maxItemsInInsert ? maxItemsInInsert : cache.Count;

                var sql = "INSERT INTO Alarms (\"TimeStamp\", \"AlarmInfoId\", \"Custom1\", \"Custom2\", \"Custom3\", \"Custom4\") VALUES ";
                for (var i = 0; i < len; i++)
                {
                    var item = cache.Dequeue();
                    sql += $"({item.TimeStamp}, {item.AlarmInfoId}, {nullIfNull(item.Custom1)}, {nullIfNull(item.Custom2)}, {nullIfNull(item.Custom3)}, {nullIfNull(item.Custom4)})" + ",";
                }

                sql = sql.Trim().Trim(',') + ';';

                await context.Database.ExecuteSqlRawAsync(sql)
                    .ConfigureAwait(false);
            }
            // ########## Конец костыля

            if (nextMaintain < DateTime.Now)
            {
                nextMaintain = DateTime.Now + MinMaintainPeriod;

                var count = await context.Alarms.LongCountAsync(cancellationToken).ConfigureAwait(false);

                if (count > StoreItemsCount)
                {
                    var countToRemove = count - StoreItemsCount + DeltaRecordsCount;
                    int ctr = countToRemove < 0 ? 0 :
                            countToRemove > int.MaxValue ? int.MaxValue :
                            (int)countToRemove;

                    var itemsToRemove = context.Alarms.Take(ctr);

                    context.Alarms.RemoveRange(itemsToRemove);

                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    await context.Database.ExecuteSqlRawAsync("VACUUM ;").ConfigureAwait(false);

                    nextMaintain = DateTime.Now + 4 * MinMaintainPeriod; // после такого можно чуть подольше не проверять кэш:)
                }
            }

            context.Dispose();
        }

        protected override async Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            // Старт (Инициализация контекста БД алармов)
            await InnitDB(cancellationToken).ConfigureAwait(false);

            var cache = new List<Alarm>(baseCapacityOfTmpList);

            var tNextSave = DateTime.Now + SavePeriod;

            // Главный цикл (проверка алармов и запись)
            while (!cancellationToken.IsCancellationRequested)
            {
                var tNextCheck = DateTime.Now + CheckPeriod;

                foreach (var cfg in Configs)
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

                            cache.Add(alarm);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.Fail(GetType().Name + ":" + ex.InnerMessage());
                    }
                }// for

                try
                {
                    if (cache.Count > 0)
                    {
                        if ((tNextSave <= DateTime.Now) || (cache.Count >= (baseCapacityOfTmpList * 4 / 5))) // 80% заполненности - чтобы избежать разрастания памяти
                        {
                            tNextSave = DateTime.Now + SavePeriod;
                            var newCache = new Queue<Alarm>(cache);
                            _ = Task.Run(async () =>
                            {
                                await SaveAsync(newCache, cancellationToken).ConfigureAwait(false);
                            });
                            cache.Clear();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.Fail(GetType().Name + ":" + ex.InnerMessage());
                }

                int tSleep = tNextCheck > DateTime.Now ? (int)(tNextCheck - DateTime.Now).TotalMilliseconds : minWaitTimeMs;

                await Task.Delay(tSleep).ConfigureAwait(false);
            }// while
        }

        /// <summary>
        /// Получение списка сконфигурированных сообщений
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
        public IEnumerable<AlarmCategory> GetCategories() =>
            localCategories;

        /// <summary>
        /// Доступ к архиву сообщений
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Alarm>> GetAlarms(AlarmFilter filter, CancellationToken cancellationToken)
        {
            var context = new AlarmContext(FileName);

            var query = from a in context.Alarms
                        select a;

            if (filter != default)
            {
                if (filter.TBegin != long.MinValue)
                    query = query.Where(a => a.TimeStamp >= filter.TBegin);

                if (filter.TEnd != long.MaxValue)
                    query = query.Where(a => a.TimeStamp <= filter.TEnd);

                if (filter.InfoIds != default)
                    query = query.Where(a => filter.InfoIds.Contains(a.AlarmInfo.Id));

                if (filter.AlarmCategoriesIds?.Count() > 0)
                    query = query.Where(a => filter.AlarmCategoriesIds.Contains(a.AlarmInfo.Category.Id));

                if (filter.FacilityAccessName != default)
                    query = query.Where(a => a.AlarmInfo.FacilityAccessName.Contains(filter.FacilityAccessName, StringComparison.OrdinalIgnoreCase));

                if (filter.DeviceName != default)
                    query = query.Where(a => a.AlarmInfo.DeviceName == filter.DeviceName);

                if (filter.Count != 0)
                    query = query.Skip(filter.Offset).Take(filter.Count);
            }

            var result = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            context.Dispose();
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

        public IEnumerable<int> InfoIds { get; set; }

        public string FacilityAccessName { get; set; }

        public string DeviceName { get; set; }

        public int Offset { get; set; } = 0;

        public int Count { get; set; } = 0;

    }
}
