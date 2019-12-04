using RPCExp.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPCExp.DbStore.Entities
{

    public class TagCfg : INameDescription, IProtocolSpecificData, ICopyFrom, IIdentity
    {
        public int Id { get; set; }

        public string ClassName { get; set; }

        public string Custom { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string Format { get; set; }

        public virtual Access Access { get; set; }

        public Common.ValueType ValueType { get; set; }

#pragma warning disable CA2227 // Свойства коллекций должны быть доступны только для чтения
        public ICollection<TagsToTagsGroups> TagsToTagsGroups { get; set; }
#pragma warning restore CA2227 // Свойства коллекций должны быть доступны только для чтения

        public int ScaleId { get; set; }

        public ScaleCfg Scale { get; set; }

        public ArchiveCfg ArchiveCfg { get; set; }

        public void CopyFrom(object original)
        {
            if (original is null)
                throw new ArgumentNullException(nameof(original));

            var src = (TagCfg)original;

            ClassName = src.ClassName;

            Custom = src.Custom;

            Name = src.Name;

            DisplayName = src.DisplayName;

            Description = src.Description;

            Format = src.Format;

            Access = src.Access;

            ValueType = src.ValueType;

            //foreach (var g in src.Groups)
            //    Groups.Add(g);

        }
    }
}