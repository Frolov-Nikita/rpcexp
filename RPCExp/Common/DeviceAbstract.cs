using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using RPCExp.Connections;

namespace RPCExp.Common
{
    /// <summary>
    /// Base class for a number of protocols implementation
    /// </summary>
    public abstract class DeviceAbstract : ServiceAbstract, INameDescription
    {
        private const int ONE_SECOND_TICKS = 10_000_000;

        public string Name { get ; set ; }

        public string Description { get; set; }
        
        /// <summary>
        /// Amount of ticks to wait then communication issue occurs.
        /// </summary>
        public long BadCommPeriod { get; set; } = 10 * ONE_SECOND_TICKS;

        /// <summary>
        /// 
        /// </summary>
        public bool UpdateInActiveTags { get; set; } = true;

        public long UpdateInActiveTagsPeriod { get; set; } = 20 * ONE_SECOND_TICKS;

        public IDictionary<string, TagsGroup> Groups { get; } = new Dictionary<string, TagsGroup>();

        public IDictionary<string, TagAbstract> Tags { get; } = new Dictionary<string, TagAbstract>();
        
        public ConnectionSourceAbstract ConnectionSource { get; set; }

        /// <summary>
        /// Определяет список тегов требующих чтения сейчас и дату-время следующего обновления
        /// </summary>
        /// <returns>Tuple(список тегов, время следующего обновления</returns>
        protected virtual (ICollection<TagAbstract> tags, long nextTime) GetPeriodicTagsForUpdate()
        {
            var nowTick = DateTime.Now.Ticks;
            var afterTick = nowTick + ONE_SECOND_TICKS;
            var nextTime = nowTick + BadCommPeriod + UpdateInActiveTagsPeriod + ONE_SECOND_TICKS;

            List<TagAbstract> retTags = new List<TagAbstract>();
            
            foreach (var tag in Tags.Values)
            {
                if ((!tag.IsActive) && (!UpdateInActiveTags))
                    continue;

                long period = (tag.Quality == TagQuality.GOOD) ?
                    tag.Period :
                    BadCommPeriod;

                if ((!tag.IsActive) && UpdateInActiveTags)
                    period = UpdateInActiveTagsPeriod;

                long tagNextTick = tag.Last + period;

                if (tagNextTick < afterTick)
                {
                    retTags.Add(tag);
                    tagNextTick = nowTick + period;
                }

                if (nextTime > tagNextTick)
                    nextTime = tagNextTick;
                
            }

            return (retTags, nextTime);
        }

        /// <summary>
        /// Вывод списка групп
        /// </summary>
        /// <returns>группа со списком имен тегов входящих в эту группу</returns>
        public virtual IDictionary< string, IEnumerable<string>> GetTagsGroups()
        {
            var r = new Dictionary<string, IEnumerable<string>>(Tags.Count);
            foreach (var t in Tags.Values)
                r.Add(t.Name, t.Groups.Values.Select(s=>s.Name));
            return r;
        }

        protected override async Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) {
                long nextTime = await PeriodicUpdate(cancellationToken).ConfigureAwait(false);
                
                // Ожидание до времени обновления следующего тега
                long waitTime = nextTime - DateTime.Now.Ticks;
                waitTime = waitTime > 10_000 ? waitTime : 10_000; // не меньше 10_000 = 1 миллисекунда
                waitTime = waitTime > 50_000_000 ? waitTime / 2 : waitTime;
                waitTime = waitTime < 50_000_000 ? waitTime : 50_000_000; // не больше 50_000_000 = 5 сек
                
                await Task.Delay((int)(waitTime / 10_000)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Периодическое чтение тегов
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Время в которое нужно запустить следующую проверку</returns>
        private async Task<long> PeriodicUpdate(CancellationToken cancellationToken)
        {
            long nextTime = 0;
            if (ConnectionSource.IsOpen)
            {
                (ICollection<TagAbstract> tags, long periodicNextTime) = GetPeriodicTagsForUpdate();
             
                nextTime = periodicNextTime;

                await Read(tags, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (!ConnectionSource.EnshureConnected())
                {
                    nextTime = DateTime.Now.Ticks + BadCommPeriod;

                    foreach (var t in Tags)
                        t.Value.SetValue(null, TagQuality.BAD_COMM_FAILURE);
                }
            }
            return nextTime;
        }

        /// <summary>
        /// Получить полное описание группы тегов
        /// </summary>
        /// <param name="groupName">Имя группы</param>
        /// <returns></returns>
        public virtual IEnumerable<object> GetGroupInfos(string groupName)
        {
            return from t in Tags.Values
                   where t.Groups.ContainsKey(groupName)
                   select t.GetInfo();
        }

        /// <summary>
        /// Получает значения группы тегов
        /// </summary>
        /// <param name="groupName">Имя группы</param>
        /// <returns></returns>
        /// <example>
        /// { "jsonrpc": "2.0", "method": "f1$Plc1.GetGroupValues", "params": ["usts2"], "id": "159"}
        /// </example>
        public virtual IEnumerable<TagData> GetGroupValues(string groupName)
        {

            if (!Groups.ContainsKey(groupName))
                return null;

            List<TagData> datas = new List<TagData>();

            Groups[groupName].Tick();

            return from t in Tags.Values
                     where t.Groups.ContainsKey(groupName)
                     select new TagData(t);
        }

        /// <summary>
        /// Получить значения нескольких тэгов
        /// </summary>
        /// <param name="tagNames">имена тэгов</param>
        /// <returns>значения</returns>
        /// <example>
        /// { "jsonrpc": "2.0", "method": "f1$Plc1.GetTagsValues", "params": [["DATA_95"]], "id": "159"}
        /// </example>
        public virtual ICollection<TagData> GetTagsValues(IEnumerable<string> tagNames)
        {
            List<TagData> datas = new List<TagData>();

            if (tagNames is null)
                return datas;

            foreach (string tagName in tagNames)
            {
                TagData td = null;

                if (Tags.ContainsKey(tagName))
                {
                    var tag = Tags[tagName];
                    td = new TagData(tag);
                }
                datas.Add(td);
            }

            return datas;
        }
        
        protected abstract Task Read(ICollection<TagAbstract> tags, CancellationToken cancellationToken);

        /// <summary>
        /// Запись значений.
        /// </summary>
        /// <param name="tagsValues">пара имя тега - значение</param>
        /// <returns>Кол-во записанных значений. Теги помеченные как "только для чтения" не будут записаны.</returns>
        /// <example>
        /// { "jsonrpc": "2.0", "method": "f1$Plc1.Write", "params": [{"UST_112":"-5"}], "id": "159"}
        /// </example>
        public async Task<int> Write(IDictionary<string, object> tagsValues)
        {
            if (tagsValues is null)
                return 0;

            IDictionary<TagAbstract, object> tags = new Dictionary<TagAbstract, object>(tagsValues.Count);

            foreach (var tv in tagsValues)
                if (Tags.ContainsKey(tv.Key))
                {
                    var tag = Tags[tv.Key];
                    if ((tag.Access == Access.ReadWrite) || (tag.Access == Access.WriteOnly))
                    {
                        var val = tag.Scale?.ScaleSrvToDev(tv.Value) ?? tv.Value;
                        tags.Add(tag, val);
                    }                        
                }

            return await Write(tags).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagsValues"></param>
        /// <returns></returns>
        protected abstract Task<int> Write(IDictionary<TagAbstract, object> tagsValues);
    }

}
