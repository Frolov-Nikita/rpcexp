using RPCExp.Connections;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Store.Entities
{
    public class ConnectionSourceWrapper : ClassWrapperAbstract<IConnectionSource>
    {
        public override IConnectionSource Unwrap()
        {
            throw new NotImplementedException();
        }

        public override void Wrap(IConnectionSource obj)
        {
            throw new NotImplementedException();
        }
    }
}
