using RPCExp.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RPCExp.Store.Entities
{
    public class TagsGroupCfg : INameDescription, ICopyFrom, IIdentity
    {
        public TagsGroupCfg() { }

        public TagsGroupCfg(TagsGroupCfg original) { CopyFrom(original); }

        public TagsGroupCfg(TagsGroup original) 
        {
            Name = original.Name;
            Description = original.Description;
            Min = original.Min;
        }

        [Key]
        public int Id { get; set; }

        public long Min { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<TagsToTagsGroups> TagsToTagsGroups { get; set; }

        public void CopyFrom(object original)
        {
            var src = (TagsGroupCfg)original;
            Name = src.Name;
            Description = src.Description;
            Min = src.Min;
        }
    }
}
