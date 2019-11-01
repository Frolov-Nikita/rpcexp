using RPCExp.Connections;
using RPCExp.Store.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RPCExp.Store.Serializers
{
    class TcpConnectionSourceSerializer : IConnectionSourceSerializer
    {
        public string ClassName => "Tcp";
        
        public ConnectionSourceCfg Pack(ConnectionSourceAbstract connectionSource)
        {
            var src = (TcpConnectionSource)connectionSource;
            var config = new ConnectionSourceCfg
            {
                Name = src.Name,
                Description = src.Description,
                ClassName = ClassName,
                Cfg = JsonConvert.SerializeObject(new
                {
                    src.Host,
                    src.Port,
                }),
            };
            return config;
        }

        public ConnectionSourceAbstract Unpack(ConnectionSourceCfg config)
        {
            var connectionSource = new TcpConnectionSource {
                Name = config.Name,
                Description = config.Description,
            };

            var jo = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(config.Cfg);

            if(jo.ContainsKey(nameof(connectionSource.Host)))
                connectionSource.Host = (string)jo[nameof(connectionSource.Host)].ToObject(typeof(string));

            if (jo.ContainsKey(nameof(connectionSource.Port)))
                connectionSource.Port = (int)jo[nameof(connectionSource.Port)].ToObject(typeof(int));

            return connectionSource;
        }
    }
}
