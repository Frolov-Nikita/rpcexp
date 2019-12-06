using System;
using System.Collections.Generic;
using System.Linq;

namespace RPCExp.Common
{
    /// <summary>
    /// 
    /// </summary>
    public enum Access { ReadOnly, ReadWrite, WriteOnly }

    /// <summary>
    /// Base class for implementation variables tags for concrete protocol
    /// </summary>
    public abstract class TagAbstract : TagData, INameDescription
    {
        private static readonly long DefaultPeriod = TimeSpan.FromSeconds(1).Ticks;

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <summary>
        /// Text to display in frontend
        /// </summary>
        public string DisplayName { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }
        
        /// <summary>
        /// Format shows how to display value.
        /// Value should be formated in frontend.
        /// Value transmit from this app as is.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Some tags be with limited access by protocol implementation.
        /// Some of them - by configuration/
        /// </summary>
        public virtual Access Access { get; set; }

        /// <summary>
        /// Type of value
        /// </summary>
        public ValueType ValueType { get; set; }

        /// <summary>
        /// Группы через которые можно опросить тег
        /// </summary>
        public IDictionary<string, TagsGroup> Groups { get; set; } = new Dictionary<string, TagsGroup>();

        /// <summary>
        /// Reference to template contains this tag
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// Необязательное масштабирование
        /// </summary>
        public Scale Scale { get; set; }

        /// <summary>
        /// Период опроса определяется как минимальный период из групп опроса
        /// </summary>
        public long Period
        {
            get
            {
                if ((Groups?.Count ?? 0) == 0)
                    return DefaultPeriod;
                return Groups.Values.Min(group => group.Period);
            }
        }

        /// <summary>
        /// Тег активен, если активна хоть одна и групп опроса тэга
        /// </summary>
        public bool IsActive
        {
            get
            {
                foreach (var s in Groups.Values)
                    if (s.IsActive)
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Information about this tag
        /// It should be cached.
        /// </summary>
        /// <returns></returns>
        public virtual object GetInfo() => new
        {
            Name,
            DisplayName,
            Description,
            Format,
            Access,
            ValueType,
            Scale?.Units,
            Groups = Groups.Keys,

            Value,
            Quality,
            LastGood,
            Last,
        };

        /// <summary>
        /// Sets value received from plc to tag.
        /// Uses scaling if it needs.
        /// Updates timestamps.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="qty"></param>
        internal void SetValue(object value, TagQuality qty = TagQuality.GOOD)
        {
            Quality = qty;
            Last = DateTime.Now.Ticks;
            if (qty == TagQuality.GOOD)
            {
                Value = Scale?.ScaleDevToSrv(value) ?? value;
                LastGood = Last;
            }
        }
    }
}
