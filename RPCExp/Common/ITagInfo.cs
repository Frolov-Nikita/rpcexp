using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{
    public interface ITagInfo
    {
        string Name { get; set; }

        string Description { get; set; }

        bool CanRead { get; set; }

        bool CanWrite { get; set; }
    }
}
