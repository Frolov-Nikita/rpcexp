using System;

namespace RPCExp.Common
{
    public interface IPeriodSource
    {
        /// <summary>
        /// Indicates that tagGroup doesn't requested too long.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// DateTime.Ticks of last Tick() call.
        /// </summary>
        long Last { get; }

        /// <summary>
        /// minimum period to limit calculated period
        /// </summary>
        long Min { get; set; }

        /// <summary>
        /// Calculated period. It should be used as period for requests values of tags in group from device.
        /// </summary>
        long Period { get; }

        /// <summary>
        /// It mast be called every time when tagGroup values are requested
        /// </summary>
        void Tick();
    }

    /// <summary>
    /// This is dummy class for providing constant period.
    /// It uses in tagGroups of alarmLogging and tagLogging.
    /// </summary>
    public class BasicPeriodSource : IPeriodSource
    {
        /// <inheritdoc/>
        public bool IsActive => true;

        /// <inheritdoc/>
        public long Last { get; private set; } = 0;

        /// <inheritdoc/>
        public long Min { get; set; } = 2 * 10_000_000; // 2 сек

        /// <inheritdoc/>
        public long Period => Min;

        /// <inheritdoc/>
        public void Tick()
        {
            Last = DateTime.Now.Ticks;
        }
    }

    /// <summary>
    /// Calculates avg timespan between several Tick() method calls.
    /// </summary>
    public class TickPeriodSource : IPeriodSource
    {
        private int i = 0;

        private long period;

        private readonly long[] t = new long[3];// { 0, 0, DateTime.Now.Ticks };

        /// <inheritdoc/>
        public long Min { get; set; } = 10_000_000; // 1 сек

        /// <summary>
        /// <inheritdoc/>
        /// if method Tick() was not call in 3 calculated period it returns false.
        /// </summary>
        public bool IsActive => DateTime.Now.Ticks < Last + 3 * Period;

        /// <inheritdoc/>
        public long Period => period > Min ? period : Min;


        /// <inheritdoc/>
        public long Last => t[i];

        /// <inheritdoc/>
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
