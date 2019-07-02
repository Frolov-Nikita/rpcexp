using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{

    public class TimeTrack
    {
        const int max = 4;
        long[] dt = new long[max];
        long avg = 0;
        long lastT = DateTime.Now.Ticks;

        public long AvgTickPeriod { get { return avg; } }

        public void Tick()
        {
            long sum = 0, max = 0;
            int cnt = 0, idNoise = 0;

            for (var i = dt.Length - 1; i >= 1; i--)
            {
                dt[i] = dt[i - 1];
                if(dt[i] != 0)
                {
                    sum += dt[i];
                    cnt++;
                }
            }
            
            if (cnt == 0) return;

            dt[0] = DateTime.Now.Ticks - lastT;
            lastT = DateTime.Now.Ticks;

            sum += dt[0];
            avg = sum/cnt;

            if ((cnt < 3) || ((dt[0] / avg) > (dt[0] / 10)))
                return;

            for (var i = 0; i < dt.Length; i++)
            {
                if (dt[i] == 0)
                    continue;

                long d = Math.Abs(dt[i] - avg);
                if(d > max)
                {
                    max = d;
                    idNoise = i;
                }
            }
            sum -= dt[idNoise];
            avg = sum / (cnt - 1);
            return;
        }
    }
}