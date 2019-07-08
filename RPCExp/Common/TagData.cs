using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{
    public class TagData
    {
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

    }


    public class TagDataStat: TagData
    {
        private readonly Ticker ticker = new Ticker();

        public long StatPeriod => ticker.Period;

        public bool StatIsAlive => ticker.IsActive;

        public override object GetValue()
        {
            ticker.Tick();
            return val;
        }

        internal object GetInternalValue() => val;

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
    }
}
