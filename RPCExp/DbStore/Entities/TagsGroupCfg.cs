using RPCExp.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPCExp.DbStore.Entities
{
    public class TagsGroupCfg : INameDescription, ICopyFrom, IIdentity
    {
        public TagsGroupCfg() { }

        public TagsGroupCfg(TagsGroupCfg original) { CopyFrom(original); }

        public TagsGroupCfg(TagsGroup original)
        {
            if (original is null)
                return;
            Name = original.Name;
            Description = original.Description;
            Min = original.Min;
        }

        [Key]
        public int Id { get; set; }

        public long Min { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<TagsToTagsGroups> TagsToTagsGroups { get; }

        public void CopyFrom(object original)
        {
            if (original is null)
                throw new ArgumentNullException(nameof(original));

            var src = (TagsGroupCfg)original;
            Name = src.Name;
            Description = src.Description;
            Min = src.Min;
        }
    }
}
