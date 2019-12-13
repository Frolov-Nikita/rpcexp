using RPCExp.Common;
using RPCExp.TagLogger.Entities;
using System;

namespace RPCExp.TagLogger
{
    /// <summary>
    /// describes when and how tags need to be archived
    /// </summary>
    public class TagLogConfig
    {
        private decimal lastVal = 0;

        private long lastTime = 0;

        /// <summary>
        /// Tag reference which value will be archived
        /// </summary>
        public TagAbstract Tag { get; private set; }

        /// <summary>
        /// information about the tag
        /// </summary>
        public TagLogInfo TagLogInfo { get; set; }

        /// <summary>
        /// Макс процент от шкалы
        /// </summary>
        public decimal Hyst { get; set; } = 0.10M;

        /// <summary>
        /// Максимальный период в секундах
        /// </summary>
        public int PeriodMaxSec { get; set; } = 600;

        /// <summary>
        /// Минимальный период в секундах
        /// </summary>
        public int PeriodMinSec { get; set; } = 1;

        private static TagsGroup TagsLogTagGroup = new TagsGroup(new BasicPeriodSource())
        {
            Name = "TagsLogTagGroup",
            Description = "Tags group to periodically check condition of logging",
            Min = 20 * 10_000_000,
        };

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tag"></param>
        public TagLogConfig(TagAbstract tag)
        {
            Tag = tag ?? throw new ArgumentNullException(nameof(tag));

            if (tag.Groups.ContainsKey(TagsLogTagGroup.Name))
                tag.Groups.AddByName(TagsLogTagGroup);
        }

        /// <summary>
        /// Property getter decide: does tag value needs to be archived now?
        /// if it does getter will return tag data to save into db.
        /// if it doesn't getter will return null
        /// </summary>
        public TagLogData NeedToArcive
        {
            get
            {
                if ((Tag == default) || Tag.Quality < TagQuality.GOOD)
                    return null;

                var now = DateTime.Now.Ticks;
                // TODO: кешировать вычисление периодов
                if (lastTime + ((long)PeriodMinSec) * 10_000_000 >= now)
                    return null;

#pragma warning disable CA1305 // Укажите IFormatProvider
                var val = (decimal)Convert.ChangeType(Tag?.Value ?? 0, typeof(decimal));
#pragma warning restore CA1305 // Укажите IFormatProvider

                if (((lastTime + ((long)PeriodMaxSec) * 10_000_000) <= now) ||
                    (Math.Abs(lastVal - val) >= Hyst))
                {
                    lastVal = val;
                    lastTime = now;
                    return new TagLogData
                    {
                        TagLogInfo = TagLogInfo,
                        TagLogInfoId = TagLogInfo.Id,
                        TimeStamp = lastTime,
                        Value = val,
                    };
                }

                return null;
            }
        }


    }
}
