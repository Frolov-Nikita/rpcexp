using RPCExp.Connections;
using RPCExp.DbStore.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RPCExp.DbStore.Serializers
{
    class SerialConnectionSourceSerializer : IConnectionSourceSerializer
    {
        public string ClassName => "Serial";
        
        public ConnectionSourceCfg Pack(ConnectionSourceAbstract connectionSource)
        {
            var src = (SerialConnectionSource)connectionSource;
            var config = new ConnectionSourceCfg
            {
                Name = src.Name,
                Description = src.Description,
                ClassName = ClassName,
                Cfg = JsonConvert.SerializeObject(new
                {
                    src.Port,
                    src.Baud,
                    src.Data,
                    src.Parity,
                    src.StopBits,
                }),
            };
            return config;
        }

        public ConnectionSourceAbstract Unpack(ConnectionSourceCfg config)
        {
            var connectionSource = new SerialConnectionSource
            {
                Name = config.Name,
                Description = config.Description,
            };

            var jo = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(config.Cfg);

            if(jo.ContainsKey(nameof(connectionSource.Port)))
                connectionSource.Port = (string)jo[nameof(connectionSource.Port)].ToObject(typeof(string));

            if (jo.ContainsKey(nameof(connectionSource.Baud)))
                connectionSource.Baud = (int)jo[nameof(connectionSource.Baud)].ToObject(typeof(int));

            if (jo.ContainsKey(nameof(connectionSource.Data)))
                connectionSource.Data = (int)jo[nameof(connectionSource.Data)].ToObject(typeof(int));

            if (jo.ContainsKey(nameof(connectionSource.Parity)))
                connectionSource.Parity = (RJCP.IO.Ports.Parity)jo[nameof(connectionSource.Parity)].ToObject(typeof(RJCP.IO.Ports.Parity));

            if (jo.ContainsKey(nameof(connectionSource.StopBits)))
                connectionSource.StopBits = (RJCP.IO.Ports.StopBits)jo[nameof(connectionSource.StopBits)].ToObject(typeof(RJCP.IO.Ports.StopBits));

            return connectionSource;
        }
    }
}
