using RPCExp.Connections;
using RPCExp.DbStore.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.DbStore.Serializers
{
    public interface IConnectionSourceSerializer
    {
       string ClassName { get; }

       ConnectionSourceCfg Pack(ConnectionSourceAbstract connectionSource);

       ConnectionSourceAbstract Unpack(ConnectionSourceCfg config);

    }
}
