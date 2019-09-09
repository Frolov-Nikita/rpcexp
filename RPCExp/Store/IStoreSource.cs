using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Store
{
    interface IStoreSource
    {
        Common.Store Get(string target);

        Common.Store Load(string target);

        void Save(string target);
    }
}
