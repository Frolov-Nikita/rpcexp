using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public interface IConfigString
    {
        void UpdateFromCfgString(string cfg);

        string ToCfgString();
    }
}
