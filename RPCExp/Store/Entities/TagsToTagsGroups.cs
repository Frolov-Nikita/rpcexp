using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Store.Entities
{
    /// <summary>
    /// Many2Many relartion
    /// </summary>
    public class TagsToTagsGroups
    {
        public int TagId { get; set; }

        public TagCfg TagCfg { get; set; }

        public int TagsGroupId { get; set; }

        public TagsGroupCfg TagsGroupCfg { get; set; }
    }
}
