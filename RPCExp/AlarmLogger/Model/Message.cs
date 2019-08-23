using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.AlarmLogger.Model
{
    public class Message
    {
        public int Id { get; set; }

        public string Condition { get; set; }

        public string Template { get; set; }

        public Category Category { get; set; }
    }
}
