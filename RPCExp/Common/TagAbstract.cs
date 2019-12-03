using System;
using System.Collections.Generic;
using System.Linq;

namespace RPCExp.Common
{
    public enum Access { ReadOnly, ReadWrite, WriteOnly}

    public abstract class TagAbstract : TagData, INameDescription
    {
        static readonly long DefaultPeriod = TimeSpan.FromSeconds(1).Ticks;

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string Units { get; set; }

        public string Format { get; set; }

        public virtual Access Access { get; set; }

        public ValueType ValueType { get; set; }

        /// <summary>
        /// Группы через которые можно опросить тег
        /// </summary>
        public IDictionary<string, TagsGroup> Groups { get; set; } = new Dictionary<string, TagsGroup>();

        public int TemplateId { get; set; }

        /// <summary>
        /// Необязательное масштабирование
        /// </summary>
        public Scale Scale { get; set; } = new Scale();

        /// <summary>
        /// Период опроса определяется как минимальный период из групп опроса
        /// </summary>
        public long Period { get {
                if ((Groups?.Count ?? 0) == 0)
                    return DefaultPeriod;
                return Groups.Values.Min( group => group.Period);
            }
        }

        /// <summary>
        /// Тег активен, если активна хоть одна и групп опроса тэга
        /// </summary>
        public bool IsActive
        { get {
                foreach (var s in Groups.Values)
                    if (s.IsActive)
                        return true;
                return false;
            }
        }

        public virtual object GetInfo() => new
        {
            Name,
            DisplayName,
            Description,
            Units,
            Format,
            Access,
            ValueType,
            Scale = Scale != null ? new { Scale.Min, Scale.Max } : null,
            Groups = Groups.Keys,
            
            Value,
            Quality,
            LastGood,
            Last,
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="qty"></param>
        internal void SetValue(object value, TagQuality qty = TagQuality.GOOD)
        {
            Quality = qty;
            Last = DateTime.Now.Ticks;
            if (qty == TagQuality.GOOD)
            {
                Value = Scale.ScaleDevToSrv(value);
                LastGood = Last;
            }
        }
    }
}
