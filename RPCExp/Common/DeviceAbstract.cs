using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
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

        public virtual IDictionary<string, TagAbstract> Tags { get; } = new Dictionary<string, TagAbstract>();

        protected ICollection<TagAbstract> NeedToUpdate(out long nextTime, bool force = false)
        {
            var nowTick = DateTime.Now.Ticks;
            var afterTick = nowTick + TimeSpan.FromSeconds(1).Ticks;
            nextTime = nowTick + BadCommWaitPeriod;
            Collection<TagAbstract> retTags = new Collection<TagAbstract>();

            foreach (var tag in Tags.Values)
            {
                if ((!tag.StatIsAlive) && (!InActiveUpdate) && (!force))
                    continue;

                long period = (tag.Quality == TagQuality.GOOD) ?
                    tag.StatPeriod :
                    BadCommWaitPeriod;

                if ((!tag.StatIsAlive) && InActiveUpdate)
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

        /// <summary>
        /// Получить значения нескольких тэгов
        /// </summary>
        /// <param name="tagNames">имена тэгов</param>
        /// <returns>значения</returns>
        [Display(Description = "Получить значения нескольких тэгов")]
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
