using System;
using System.Collections.Generic;
using System.Linq;

namespace RPCExp.Common
{
    public abstract class TagAbstract : TagData, INameDescription
    {
        static long DefaultPeriod = TimeSpan.FromSeconds(1).Ticks;

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string Units { get; set; }

        public string Format { get; set; }

        public abstract bool CanWrite { get; set; }

        public ValueType ValueType { get; set; }

        /// <summary>
        /// Группы через которые можно опросить тег
        /// </summary>
        public IDictionary<string, TagsGroup> Groups { get; set; }
        
        /// <summary>
        /// Необязательное масштабирование
        /// </summary>
        public Scale Scale { get; set; }

        /// <summary>
        /// Период опроса определяется как минимальный период и изгупп опроса
        /// </summary>
        public long Period { get {
                if ((Groups?.Count ?? 0) == 0)
                    return DefaultPeriod;
                return Groups.Values.Min(s=>s.Period);
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
            CanWrite,
            ValueType,
            Scale = Scale != null ? new { Scale.Min, Scale.Max } : null,
            Groups = Groups.Keys,
            
            Value,
            Quality,
            LastGood,
            Last,
        };
    }
}
