using System;
using System.Collections.Generic;
using System.Text;
using RPCExp.Common;

namespace RPCExp.Store
{
    public class SqliteStoreSource : IStoreSource
    {
        public Common.Store Get(string target)
        {
            throw new NotImplementedException();
        }

        public Common.Store Load(string target)
        {
            var context = new StoreContext(target);
            throw new NotImplementedException();

        }

        public void Save(string target)
        {
            throw new NotImplementedException();
        }
    }
}
