using RPCExp.Common;
using System;
using System.Collections.Generic;

namespace RPCExp.Store.Entities
{

    public class TagCfg : INameDescription, IProtocolSpecificData
    {
        public int Id { get; set; }
        public string ClassName { get; set; }

        public string Custom { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string Units { get; set; }

        public string Format { get; set; }

        public decimal ScaleDevMin { get; set; } = -32768;

        public decimal ScaleDevMax { get; set; } = 32767;

        public decimal ScaleMin { get; set; } = -32768;

        public decimal ScaleMax { get; set; } = 32767;

        public virtual Access Access { get; set; }

        public Common.ValueType ValueType { get; set; }

        public ICollection<TagsGroup> Groups { get; set; }
    }
}