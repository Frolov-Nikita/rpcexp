using RPCExp.Common;
using RPCExp.TagLogger.Entities;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace RPCExp.TagLogger
{
    public class TagLogService : ServiceAbstract
    {
        TagLogContext context;

        public TagLogService()
        {

        }

        ~TagLogService() =>
            context?.SaveChanges();

        public int ItemsToSaveLimit { get; private set; } = 1;

        private TagLogContext Context {
            get {
                if (context == default)
                    context = new TagLogContext();
                return context;
            }
        }

        public List<TagLogConfig> Configs { get; set; } = new List<TagLogConfig>();

        protected override async Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            // Старт (Инициализация контекста БД алармов)
            int itemsToSaveCount = 0;
            foreach (var cfg in Configs)
            {
                var storedTagLogInfo = Context.TagLogInfo.FirstOrDefault(e =>
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
                    Context.TagLogInfo.Add(storedTagLogInfo);
                    itemsToSaveCount++;
                }
                cfg.TagLogInfo = storedTagLogInfo;
            }

            if (itemsToSaveCount >= ItemsToSaveLimit)
            {
                await Context.SaveChangesAsync(cancellationToken);
                itemsToSaveCount = 0;
            }

            // Главный цикл (проверка алармов и запись)
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var cfg in Configs)
                    {
                        var archiveData = cfg.NeedToArcive;
                        if (archiveData != default)
                        {
                            await Context.TagLogData.AddAsync(new TagLogData
                            {
                                TagLogInfo = cfg.TagLogInfo,
                                TimeStamp = archiveData.TimeStamp,
                                Value = archiveData.Value,
                            });
                            itemsToSaveCount++;
                        }                            
                    }

                    if (itemsToSaveCount >= ItemsToSaveLimit)
                    {
                        await Context.SaveChangesAsync(cancellationToken);
                        itemsToSaveCount = 0;
                    }

                    await Task.Delay(100);
                }
                catch//(Exception ex)
                {
                    await Task.Delay(1_000);
                }
            }

            // Завершение
            await Context.SaveChangesAsync(cancellationToken);
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
        public IEnumerable<TagLogData> GetData(IEnumerable<int> ids, long tBegin = long.MinValue, long tEnd = long.MaxValue)
        {
            return from d in Context.TagLogData
                    where ids.Contains(d.TagLogInfoId) &&
                    d.TimeStamp >= tBegin &&
                    d.TimeStamp <= tEnd
                    select d;
        }
    }
}
