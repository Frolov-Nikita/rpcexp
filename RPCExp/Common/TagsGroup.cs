using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RPCExp.Common
{
    public class TagsGroup: Ticker, INameDescription
    {
        public TagsGroup()
        {
            Name = Guid.NewGuid().ToString();
        }

        public TagsGroup(TagsGroup tagsSet)
        {
            Name = tagsSet.Name;
            Description = tagsSet.Description;
        }

        [Key]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
