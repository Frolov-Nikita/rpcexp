using System;
using System.Collections.Generic;
using System.Linq;

namespace RPCExp.Common
{
    public abstract class TagAbstract : TagData, INameDescription
    {
        static long DefaultPeriod = TimeSpan.FromSeconds(1).Ticks;

        public string Name { get; set; }

        public string Description { get; set; }

        public abstract bool CanWrite { get; set; }

        public IDictionary<string, TagsSet> Sets { get; set; }

        public long Period { get {
                if ((Sets?.Count ?? 0) == 0)
                    return DefaultPeriod;
                return Sets.Values.Min(s=>s.Period);
            }
        }

        public bool IsActive
        { get {
                foreach (var s in Sets.Values)
                    if (s.IsActive)
                        return true;
                return false;
            }
        }
    }
}
