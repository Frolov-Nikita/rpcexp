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
            val = tagData.GetValue();
            Last = tagData.Last;
            LastGood = tagData.LastGood;
        }

        protected object val;

        public TagQuality Quality { get; protected set; } = TagQuality.BAD;

        public long Last { get; protected set; } = DateTime.Now.Ticks;

        public long LastGood { get; protected set; } = DateTime.Now.Ticks;

        public object Value => GetValue();

        public virtual object GetValue() => val;

        internal void SetValue(object value, TagQuality qty = TagQuality.GOOD)
        {
            Quality = qty;
            Last = DateTime.Now.Ticks;
            if (qty == TagQuality.GOOD)
            {
                val = value;
                LastGood = Last;
            }
        }

        //public static int Hash(string val)
        //{
        //    int h = 0;
        //    for (var i = 0; i < val.Length; i++)
        //        h += (h << 8) ^ val[i];
        //    return h;
        //}

        public string ToJson()
        {
            long tsLast = (Last - ZeroTick) / 10_000; // миллисекунды с начала времен (1970)
            var result = $"[\"{val.ToString()}\",\"{ Quality}\",{tsLast}";
            
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
