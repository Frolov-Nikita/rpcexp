using System;
using System.ComponentModel.DataAnnotations;

namespace RPCExp.Common
{
    /// <summary>
    /// Represent group of the tags which infos or data can be requested by single query from frontend.
    /// This entity doesn't apply to communication with devices.
    /// It also provide the period of update tags in this group.
    /// </summary>
    public class TagsGroup : IPeriodSource, INameDescription
    {
        private readonly IPeriodSource periodSource = new TickPeriodSource();

        /// <summary>
        /// ctor initialize Group name by new guid value.
        /// </summary>
        public TagsGroup()
        {
            Name = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// ctor allows to create Group with custom updatePeriod "calculator"
        /// </summary>
        /// <param name="periodSource"></param>
        public TagsGroup(IPeriodSource periodSource)
        {
            this.periodSource = periodSource;
            Name = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Копирующий конструктор
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


        /// <inheritdoc/>
        [Key]
        public string Name { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <summary>
        /// Encapsulates periodSource.IsActive property of updatePeriod "calculator".
        /// It should be false if groups data or info doesn't requested too long.
        /// </summary>
        public bool IsActive => periodSource.IsActive;

        /// <summary>
        /// Encapsulates periodSource.Last property of updatePeriod "calculator".
        /// It shows when data of this group was requested.
        /// </summary>
        public long Last => periodSource.Last;

        /// <summary>
        /// Encapsulates periodSource.Min property of updatePeriod "calculator".
        /// </summary>
        public long Min { get => periodSource.Min; set { periodSource.Min = value; } }

        /// <summary>
        /// Encapsulates periodSource.Period property of updatePeriod "calculator".
        /// </summary>
        public long Period => periodSource.Period;

        /// <summary>
        /// Encapsulates periodSource.Tick() method of updatePeriod "calculator".
        /// </summary>
        public void Tick() => periodSource.Tick();
    }
}
