using RPCExp.Connections;
using RPCExp.DbStore.Entities;

namespace RPCExp.DbStore.Serializers
{
    public interface IConnectionSourceSerializer
    {
        string ClassName { get; }

        ConnectionSourceCfg Pack(ConnectionSourceAbstract connectionSource);

        ConnectionSourceAbstract Unpack(ConnectionSourceCfg config);

    }
}
