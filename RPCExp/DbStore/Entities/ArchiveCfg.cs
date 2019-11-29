using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPCExp.DbStore.Entities
{
    public class ArchiveCfg: INameDescription, ICopyFrom, IIdentity
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }

        public TagCfg Tag { get; set; }

        public int TagCfgId { get; set; }

        public int PeriodMaxSec { get; set; }

        public int PeriodMinSec { get; set; }

        [Column(TypeName = "DECIMAL")]
        public decimal Hyst { get; set; }

        public void CopyFrom(object original)
        {
            if (original is null)
                throw new ArgumentNullException(nameof(original));

            ArchiveCfg src = (ArchiveCfg)original;

            Name = src.Name;
            Description = src.Description;
            Tag = src.Tag;
            PeriodMaxSec = src.PeriodMaxSec;
            PeriodMinSec = src.PeriodMinSec;
            Hyst = src.Hyst;

        }
    }
}