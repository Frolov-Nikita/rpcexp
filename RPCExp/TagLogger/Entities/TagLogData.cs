using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.TagLogger.Entities
{
    public class TagLogData
    {
        public int TagLogInfoId { get; set; }

        public TagLogInfo TagLogInfo { get; set; }
        
        public long TimeStamp { get; set; }

        public decimal Value { get; set; }
    }
}
