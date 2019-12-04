using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPCExp.DbStore.Entities
{
    public class FacilityCfg : ICopyFrom, IIdentity
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(512)]
        public string AccessName { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        [MaxLength(512)]
        public string Description { get; set; }

        public ICollection<DeviceCfg> Devices { get; } = new List<DeviceCfg>();

        public void CopyFrom(object original)
        {
            if (original is null)
                throw new ArgumentNullException(nameof(original));

            var src = (FacilityCfg)original;
            AccessName = src.AccessName;
            Name = src.Name;
            Description = src.Description;
        }
    }
}
