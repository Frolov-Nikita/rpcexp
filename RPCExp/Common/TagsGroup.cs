using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RPCExp.Common
{
    // TODO: Это, возможно, не правильно, так как переменные должны обновляться еще для архиватора и алармлоггера! 

    public class TagsGroup: Ticker, INameDescription
    {
        public TagsGroup()
        {
            Name = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// копирующий конструктор
        /// </summary>
        /// <param name="tagsGroupOriginal"></param>
        public TagsGroup(TagsGroup tagsGroupOriginal)
        {
            Name = tagsGroupOriginal.Name;
            Description = tagsGroupOriginal.Description;
            Min = tagsGroupOriginal.Min;
        }

        [Key]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
