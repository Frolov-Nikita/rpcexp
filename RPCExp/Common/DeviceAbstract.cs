using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace RPCExp.Common
{

    public abstract class DeviceAbstract : ServiceAbstract, IDevice
    {
        public virtual string Name { get ; set ; }

        public string Description { get; set; }
        
        public long BadCommWaitPeriod { get; set; } = 10 * 10_000_000;

        public bool InActiveUpdate { get; set; } = true;

        public long InActiveUpdatePeriod { get; set; } = 20 * 10_000_000;

        public IDictionary<string, TagsSet> Sets { get; set; }

        public virtual IDictionary<string, TagAbstract> Tags { get; } = new Dictionary<string, TagAbstract>();

        protected ICollection<TagAbstract> NeedToUpdate(out long nextTime, bool force = false)
        {
            var nowTick = DateTime.Now.Ticks;
            var afterTick = nowTick + TimeSpan.FromSeconds(1).Ticks;
            nextTime = nowTick + BadCommWaitPeriod;
            List<TagAbstract> retTags = new List<TagAbstract>();
            
            foreach (var tag in Tags.Values)
            {
                if ((!tag.IsActive) && (!InActiveUpdate) && (!force))
                    continue;

                long period = (tag.Quality == TagQuality.GOOD) ?
                    tag.Period :
                    BadCommWaitPeriod;

                if ((!tag.IsActive) && InActiveUpdate)
                    period = InActiveUpdatePeriod;

                long tagNextTick = tag.Last + period;

                if (tagNextTick > afterTick)
                {
                    if (nextTime > tagNextTick)
                        nextTime = tagNextTick;
                    if (!force)
                        continue;
                }
                else
                {
                    tagNextTick = nowTick + period;
                    if (nextTime > tagNextTick)
                        nextTime = tagNextTick;
                }
                retTags.Add(tag);
            }

            return retTags;
        }

        public virtual IEnumerable<string> GetSetTags(string setName)
        {
            return from t in Tags.Values
                   where t.Sets.ContainsKey(setName)
                   select t.Name;
        }

        /// <summary>
        /// Получает значения набора переменных
        /// </summary>
        /// <param name="setName"></param>
        /// <returns></returns>
        public virtual IEnumerable<TagData> GetSetValues(string setName)
        {
            if (!Sets.ContainsKey(setName))
                return null;

            List<TagData> datas = new List<TagData>();

            Sets[setName].Tick();

            return from t in Tags.Values
                     where t.Sets.ContainsKey(setName)
                     select new TagData(t);
        }

        /// <summary>
        /// Получить значения нескольких тэгов
        /// </summary>
        /// <param name="tagNames">имена тэгов</param>
        /// <returns>значения</returns>
        public virtual ICollection<TagData> GetTagsValues(Collection<string> tagNames)
        {
            List<TagData> datas = new List<TagData>();
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

        public abstract Task<int> Write(IDictionary<string, object> tagsValues);
    }


}
