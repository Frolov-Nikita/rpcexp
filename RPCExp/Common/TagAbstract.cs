using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{
    public abstract class TagAbstract : TagData, ITagInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public abstract bool CanRead { get; set; }
        public abstract bool CanWrite { get; set; }


    }
}
