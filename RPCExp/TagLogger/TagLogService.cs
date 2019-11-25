using RPCExp.Common;
using RPCExp.TagLogger.Entities;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;

namespace RPCExp.TagLogger
{
    public class TagLogService : ServiceAbstract
    {
        const int baseCapacityOfTmpList = 32; // Начальная емкость промежуточного хранилища
        
        const int minWaitTimeMs = 50; // Минимальное время ожидания, мсек

        public TimeSpan MinMaintainPeriod { get; set; } = TimeSpan.FromSeconds(10);

        DateTime nextMaintain = DateTime.Now;

        public TimeSpan CheckPeriod { get; set; } = TimeSpan.FromMilliseconds(500);

        public TimeSpan SavePeriod { get; set; } = TimeSpan.FromSeconds(10);

        public long StoreItemsCount { get; set; } = 10_000_000;
        
        long DeltaRecordsCount => 1 + StoreItemsCount * 5 / 100;

        public string FileName { get; set; } = "alarmLog.sqlite3";
        
        public List<TagLogConfig> Configs { get; } = new List<TagLogConfig>();

        private async Task InnitDB(CancellationToken cancellationToken)
        {
            var context = new TagLogContext(FileName);

            var storedInfo = await context.TagLogInfo.ToListAsync(cancellationToken).ConfigureAwait(false);

            foreach (var cfg in Configs)
            {
                var storedTagLogInfo = context.TagLogInfo.FirstOrDefault(e =>
                    e.FacilityAccessName == cfg.TagLogInfo.FacilityAccessName &&
                    e.DeviceName == cfg.TagLogInfo.DeviceName &&
                    e.TagName == cfg.TagLogInfo.TagName);

                if (storedTagLogInfo == default)
                {
                    storedTagLogInfo = new TagLogInfo
                    {
                        FacilityAccessName = cfg.TagLogInfo.FacilityAccessName,
                        DeviceName = cfg.TagLogInfo.DeviceName,
                        TagName = cfg.TagLogInfo.TagName,
                    };
                    context.TagLogInfo.Add(storedTagLogInfo);
                    storedInfo.Add(storedTagLogInfo);
                }
                cfg.TagLogInfo = storedTagLogInfo;
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            context.Dispose();
        }

        private async Task SaveAsync(Queue<TagLogData> cache, CancellationToken cancellationToken)
        {
            if ((cache?.Count ?? 0) == 0)
                return;

            System.Diagnostics.Debug.WriteLine($"TagLog.SaveAsync {cache.Count}");

            var context = new TagLogContext(FileName);

            /* // Код как оно должно работать
            context.TagLogData.AddRange(cache);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            */

            // ########## Начало костыля
            // TODO: при новых версиях EF Core (> 3.0.1) пробовать убрать этот костыль
            const int maxItemsInInsert = 128;
            while (cache.Count > 0)
            {
                var len = cache.Count > maxItemsInInsert ? maxItemsInInsert : cache.Count;

                var sql = "INSERT INTO TagLogData (\"TimeStamp\", \"TagLogInfoId\", \"Value\") VALUES ";
                for (var i=0; i< len; i++)
                {
                    var item = cache.Dequeue();
                    sql += $"({item.TimeStamp}, {item.TagLogInfoId}, {item.Value})" + ",";
                }
                
                sql = sql.Trim().Trim(',') + ';';

                await context.Database.ExecuteSqlRawAsync(sql).ConfigureAwait(false);
            }
            // ########## Конец костыля
            
            if (nextMaintain < DateTime.Now)
            {
                nextMaintain = DateTime.Now + MinMaintainPeriod;

                var count = await context.TagLogData.LongCountAsync(cancellationToken).ConfigureAwait(false);

                if (count > StoreItemsCount)
                {
                    var countToRemove = count - StoreItemsCount + DeltaRecordsCount;
                    int ctr = countToRemove < 0 ? 0 :
                            countToRemove > int.MaxValue ? int.MaxValue :
                            (int)countToRemove;

                    var itemsToRemove = context.TagLogData.Take(ctr);

                    context.TagLogData.RemoveRange(itemsToRemove);

                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    await context.Database.ExecuteSqlRawAsync("VACUUM ;").ConfigureAwait(false);

                    nextMaintain = DateTime.Now + 4 * MinMaintainPeriod; // после такого можно чуть подольше не проверять:)
                }
            }

            System.Diagnostics.Debug.WriteLine("TagLog.SaveAsync disposing");

            context.Dispose();
        }


        protected override async Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            // Старт (Инициализация контекста БД алармов)
            await InnitDB(cancellationToken).ConfigureAwait(false);

            var cache = new List<TagLogData>(baseCapacityOfTmpList);

            var tNextSave = DateTime.Now + SavePeriod;

            // Главный цикл (проверка алармов и запись)
            while (!cancellationToken.IsCancellationRequested)
            {
                var tNextCheck = DateTime.Now + CheckPeriod;

                foreach (var cfg in Configs)
                {
                    try
                    {
                        var archiveData = cfg.NeedToArcive;
                        if (archiveData != default)
                        {
                            cache.Add(new TagLogData
                            {
                                //TagLogInfo = cfg.TagLogInfo,
                                TagLogInfoId = cfg.TagLogInfo.Id,
                                TimeStamp = archiveData.TimeStamp,
                                Value = archiveData.Value,
                            });
                        }
                    }
                    catch//(Exception ex)
                    {
                        //TODO: log this exception
                    }
                }//for

                try
                {
                    if (cache.Count > 0)
                    {
                        if ((tNextSave <= DateTime.Now) || (cache.Count >= (baseCapacityOfTmpList * 4 / 5))) // 80% заполненности - чтобы избежать разрастания памяти
                        {
                            tNextSave = DateTime.Now + SavePeriod;
                            var newCache = new Queue<TagLogData>(cache);
                            _ = Task.Run(async () => {
                                await SaveAsync(newCache, cancellationToken).ConfigureAwait(false);
                            });
                            cache.Clear();
                        }
                    }
                }
                catch//(Exception ex)
                {
                    //TODO: log this exception
                }

                int tSleep = tNextCheck > DateTime.Now ? (int)(tNextCheck - DateTime.Now).TotalMilliseconds : minWaitTimeMs;

                await Task.Delay(tSleep).ConfigureAwait(false);
            }//while
        }

        /// <summary>
        /// Получение информации о хранящихся в архиве переменных.
        /// Id этих параметров используются в запросе архивных данных.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TagLogInfo> GetInfos()
        {
            return from cfg in Configs 
                   select cfg.TagLogInfo;
        }

        /// <summary>
        /// Получить архивные данные.
        /// </summary>
        /// <param name="ids">Идентификаторы параметров</param>
        /// <param name="tBegin">Время начала для выборки</param>
        /// <param name="tEnd">время окончания выборки</param>
        /// <returns></returns>
        public async Task<IEnumerable<TagLogData>> GetData(TagLogFilter filter, CancellationToken cancellationToken)
        {
            var context = new TagLogContext(FileName);

            var query = from a in context.TagLogData
                        select a;

            if (filter != default)
            {
                if (filter.TBegin != long.MinValue)
                    query = query.Where(a => a.TimeStamp >= filter.TBegin);

                if (filter.TEnd != long.MaxValue)
                    query = query.Where(a => a.TimeStamp <= filter.TEnd);

                if (filter.InfoIds != default)
                    query = query.Where(a => filter.InfoIds.Contains(a.TagLogInfo.Id));
                
                //TODO: остальное
                if (filter.Count != 0)
                    query = query.Skip(filter.Offset).Take(filter.Count);
            }

            var result = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            context.Dispose();
            return result;
        }
    }

    public class TagLogFilter
    {
        public long TBegin { get; set; } = long.MinValue;

        public long TEnd { get; set; } = long.MaxValue;

        public IEnumerable<int> InfoIds { get; set; }

        public IEnumerable<string> FacilityAccessName { get; set; }

        public IEnumerable<string> DeviceName { get; set; }

        public IEnumerable<string> TagName { get; set; }

        public int Offset { get; set; } = 0;

        public int Count { get; set; } = 0;
    }
}
