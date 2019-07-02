using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{
    public class Ticker
    {
        private byte cnt = 0;

        private readonly long[] t = new long[3];// { 0, 0, DateTime.Now.Ticks };

        public long Min { get; set; } = 10_000_000;

        public long Cache { get; set; } = 20_000_000;

        public long Max { get; set; } = 100_000_000;

        public bool IsActive
        {
            get
            {
                var p = 3 * (Period);
                p = p > Cache ? p : Cache;
                return DateTime.Now.Ticks < Last + p;
            }
        }

        public long Period { get; private set; } = 0;

        public long Last => t[t.Length - 1];

        public long Next => Last + (IsActive ? Period : Max);

        public void Tick()
        {
            int i = 0;
            for (; i < t.Length - 1;)
                t[i] = t[++i];
            
            t[i] = DateTime.Now.Ticks;

            if (cnt < t.Length)
                if (++cnt < 2)
                    return;

            Period = (t[i] - t[t.Length - cnt])/cnt;
        }
    }
}
