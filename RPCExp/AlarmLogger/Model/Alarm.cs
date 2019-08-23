using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.AlarmLogger.Model
{
    public class Alarm
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public MessageTemplate Template { get; set; }
        public string Custom1 { get; set; }
        public string Custom2 { get; set; }
        public string Custom3 { get; set; }
        public string Custom4 { get; set; }

    }
}
