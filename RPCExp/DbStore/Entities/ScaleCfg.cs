using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RPCExp.DbStore.Entities
{
    public class ScaleCfg :INameDescription
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
        
        [Column(TypeName = "DECIMAL")]
        public decimal DevMin { get; set; } = -32768;

        [Column(TypeName = "DECIMAL")]
        public decimal DevMax { get; set; } = 32767;

        [Column(TypeName = "DECIMAL")]
        public decimal Min { get; set; } = -32768;

        [Column(TypeName = "DECIMAL")]
        public decimal Max { get; set; } = 32767;

        public string Units { get; set; }
    }
}
