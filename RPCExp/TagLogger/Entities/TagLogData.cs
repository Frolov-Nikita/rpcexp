using System.ComponentModel.DataAnnotations.Schema;

namespace RPCExp.TagLogger.Entities
{
    public class TagLogData
    {
        public long TimeStamp { get; set; }

        public int TagLogInfoId { get; set; }

        public TagLogInfo TagLogInfo { get; set; }

        [Column(TypeName = "DECIMAL")]
        public decimal Value { get; set; }
    }
}
