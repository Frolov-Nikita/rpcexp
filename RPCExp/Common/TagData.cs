using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{
    public class TagData
    {
        private readonly Ticker ticker = new Ticker();

        private object val;

        public TagQuality Quality { get; private set; } = TagQuality.BAD;

        public long Last { get; private set; } = DateTime.Now.Ticks;

        public long LastGood { get; private set; } = DateTime.Now.Ticks;

        public long StatPeriod => ticker.Period;

        public bool StatIsAlive => ticker.IsActive;

        public object GetValue()
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
