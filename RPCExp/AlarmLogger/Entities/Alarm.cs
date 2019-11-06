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

        public string CustomTag1 { get; set; } = "";

        public string CustomTag2 { get; set; } = "";

        public string CustomTag3 { get; set; } = "";

        public string CustomTag4 { get; set; } = "";

    }
}
