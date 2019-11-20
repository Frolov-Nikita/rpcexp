using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{
    public class TagData
    {
        const long ZeroTick = 621_355_968_000_000_000; //1970.01.01 00:00:00.000

        public TagData(TagData tagData = null)
        {
            if (tagData == null) return;
            Quality = tagData.Quality;
            Value = tagData.Value;
            Last = tagData.Last;
            LastGood = tagData.LastGood;
        }

        public TagQuality Quality { get; protected set; } = TagQuality.BAD;

        public long Last { get; protected set; } = DateTime.Now.Ticks;

        public long LastGood { get; protected set; } = DateTime.Now.Ticks;

        public object Value { get; private set; }
        
        internal void SetValue(object value, TagQuality qty = TagQuality.GOOD)
        {
            Quality = qty;
            Last = DateTime.Now.Ticks;
            if (qty == TagQuality.GOOD)
            {
                Value = value;
                LastGood = Last;
            }
        }

        public string ToJson()
        {
            long tsLast = (Last - ZeroTick) / 10_000; // миллисекунды с начала времен (1970)
            var result = $"[\"{Value.ToString()}\",\"{ Quality}\",{tsLast}";
            
            if (Quality < TagQuality.GOOD)
            {
                long tsLastGood = (LastGood - ZeroTick) / 10_000;
                result += $",{tsLastGood}";
            }
            
            result += "]"; 
            return result;
        }
    }
}
