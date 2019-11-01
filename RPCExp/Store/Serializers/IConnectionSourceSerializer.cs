using RPCExp.Connections;
using RPCExp.Store.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Store.Serializers
{
    public interface IConnectionSourceSerializer
    {
       string ClassName { get; }

       ConnectionSourceCfg Pack(ConnectionSourceAbstract connectionSource);

       ConnectionSourceAbstract Unpack(ConnectionSourceCfg config);

    }
}
