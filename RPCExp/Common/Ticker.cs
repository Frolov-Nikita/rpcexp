using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{
    public class Ticker
    {
        //private byte cnt = 0;
        private int i = 0;

        private long period;

        private readonly long[] t = new long[3];// { 0, 0, DateTime.Now.Ticks };

        public long Min { get; set; } = 10_000_000; // 1 сек

        public bool IsActive => DateTime.Now.Ticks < Last + 3 * Period;

        public long Period {
            get => period > Min ? period : Min;
        }
        public long Last => t[i];

        public void Tick()
        {
            i = (i + 1) % t.Length;

            long s = t[i];
            t[i] = DateTime.Now.Ticks;
            if(s == 0)
                period = (t[i] - t[1]) / (i != 0 ? i : t.Length );
            else
                period = (t[i] - s) / t.Length;

        }
    }
}
