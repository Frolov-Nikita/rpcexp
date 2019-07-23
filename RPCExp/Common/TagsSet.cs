using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{
    public class TagsSet: Ticker, INameDescription
    {
        public TagsSet(TagsSet tagsSet = null)
        {
            if (tagsSet == null)
                return;
            Name = tagsSet.Name;
            Description = tagsSet.Description;
        }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
