using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.AlarmLogger.Entities
{
    public class Alarm
    {
        public int AlarmInfoId { get; set; }

        public AlarmInfo AlarmInfo { get; set; }

        public long TimeStamp { get; set; }

        public string Custom1 { get; set; } = "";

        public string Custom2 { get; set; } = "";

        public string Custom3 { get; set; } = "";

        public string Custom4 { get; set; } = "";

    }
}
