using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RPCExp.Common
{

    public class TagsGroup : IPeriodSource, INameDescription
    {
        readonly IPeriodSource periodSource = new TickPeriodSource();

        public TagsGroup()
        {
            Name = Guid.NewGuid().ToString();
        }

        public TagsGroup(IPeriodSource periodSource)
        {
            this.periodSource = periodSource;
            Name = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// копирующий конструктор
        /// </summary>
        /// <param name="tagsGroupOriginal"></param>
        public TagsGroup(TagsGroup tagsGroupOriginal)
        {
            if (tagsGroupOriginal is null)
                return;

            Name = tagsGroupOriginal.Name;
            Description = tagsGroupOriginal.Description;
            Min = tagsGroupOriginal.Min;
        }

        [Key]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsActive => periodSource.IsActive;

        public long Last => periodSource.Last;

        public long Min { get => periodSource.Min; set { periodSource.Min = value; } }

        public long Period => periodSource.Period;

        public void Tick() => periodSource.Tick();
    }
}
