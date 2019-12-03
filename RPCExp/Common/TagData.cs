using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{
    /// <summary>
    /// Represents runtime data of the tag.
    /// It used in json serialization.
    /// </summary>
    public class TagData
    {
        /// <summary>
        /// Copying ctor
        /// </summary>
        /// <param name="tagData"></param>
        public TagData(TagData tagData = null)
        {
            if (tagData == null) return;
            Quality = tagData.Quality;
            Value = tagData.Value;
            Last = tagData.Last;
            LastGood = tagData.LastGood;
        }

        /// <summary>
        /// Quality of tag's data
        /// </summary>
        public TagQuality Quality { get; protected set; } = TagQuality.BAD;

        /// <summary>
        /// DateTime.Ticks when tags data was updated.
        /// </summary>
        public long Last { get; protected set; } = DateTime.Now.Ticks;

        /// <summary>
        /// DateTime.Ticks when tags data was updated by data of good quality.
        /// </summary>
        public long LastGood { get; protected set; } = DateTime.Now.Ticks;

        /// <summary>
        /// Last updated value
        /// </summary>
        public object Value { get; protected set; }

    }
}
