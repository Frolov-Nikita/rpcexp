using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.DbStore.Entities
{
    /// <summary>
    /// Many2Many relation
    /// </summary>
    public class TagsToTagsGroups
    {
        public int TagId { get; set; }

        public TagCfg TagCfg { get; set; }

        public int TagsGroupId { get; set; }

        public TagsGroupCfg TagsGroupCfg { get; set; }
    }
}
