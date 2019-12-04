using System;

namespace RPCExp.Common
{
    public interface IPeriodSource
    {
        bool IsActive { get; }
        long Last { get; }
        long Min { get; set; }
        long Period { get; }

        void Tick();
    }

    public class BasicPeriodSource : IPeriodSource
    {
        public bool IsActive => true;

        public long Last { get; private set; } = 0;

        public long Min { get; set; } = 2 * 10_000_000; // 2 сек

        public long Period => Min;

        public void Tick()
        {
            Last = DateTime.Now.Ticks;
        }
    }

    public class TickPeriodSource : IPeriodSource
    {
        //private byte cnt = 0;
        private int i = 0;

        private long period;

        private readonly long[] t = new long[3];// { 0, 0, DateTime.Now.Ticks };

        public long Min { get; set; } = 10_000_000; // 1 сек

        public bool IsActive => DateTime.Now.Ticks < Last + 3 * Period;

        public long Period
        {
            get => period > Min ? period : Min;
        }
        public long Last => t[i];

        public void Tick()
        {
            var now = DateTime.Now.Ticks;
            if ((Last + Min) >= now)
                return; // игнорим тики, которые быстрее минимума.

            i = (i + 1) % t.Length;

            long s = t[i];
            t[i] = now;
            if (s == 0)
                period = (t[i] - t[1]) / (i != 0 ? i : t.Length);
            else
                period = (t[i] - s) / t.Length;

        }
    }
}
