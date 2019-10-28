using RPCExp.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RPCExp.Store.Entities
{
    class TagsGroupCfg : INameDescription, ICopyFrom, IIdentity
    {
        public TagsGroupCfg() { }

        public TagsGroupCfg(TagsGroupCfg original) { CopyFrom(original); }

        public TagsGroupCfg(TagsGroup original) 
        {
            Name = original.Name;
            Description = original.Description;
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public void CopyFrom(object original)
        {
            var src = (TagsGroupCfg)original;
            Name = src.Name;
            Description = src.Description;
        }
    }
}
