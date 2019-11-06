using RPCExp.Common;
using System;
using System.Collections.Generic;

namespace RPCExp.Store.Entities
{

    public class TagCfg : INameDescription, IProtocolSpecificData, ICopyFrom, IIdentity
    {
        public int Id { get; set; }

        public string ClassName { get; set; }

        public string Custom { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string Units { get; set; }

        public string Format { get; set; }

        public decimal ScaleDevMin { get; set; } = -32768;

        public decimal ScaleDevMax { get; set; } = 32767;

        public decimal ScaleMin { get; set; } = -32768;

        public decimal ScaleMax { get; set; } = 32767;

        public virtual Access Access { get; set; }

        public Common.ValueType ValueType { get; set; }

        public ICollection<TagsToTagsGroups> TagsToTagsGroups { get; set; }

        public ArchiveCfg ArchiveCfg { get; set; }

        public void CopyFrom(object original)
        {
            var src = (TagCfg)original;

            ClassName = src.ClassName;

            Custom = src.Custom;

            Name = src.Name;

            DisplayName = src.DisplayName;

            Description = src.Description;

            Units = src.Units;

            Format = src.Format;

            ScaleDevMin = src.ScaleDevMin;

            ScaleDevMax = src.ScaleDevMax;

            ScaleMin = src.ScaleMin;

            ScaleMax = src.ScaleMax;

            Access = src.Access;

            ValueType = src.ValueType;

            //foreach (var g in src.Groups)
            //    Groups.Add(g);

        }
    }
}