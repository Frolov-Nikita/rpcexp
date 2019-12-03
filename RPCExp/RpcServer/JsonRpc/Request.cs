using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RPCExp.RpcServer.JsonRpc
{
    class Request
    {
        //{"jsonrpc": "2.0", "method": "subtract", "params": {"subtrahend": 23, "minuend": 42}, "id": 3}

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("jsonrpc")]
        public string Version { get; set; } = "2.0";

        [JsonProperty("method")]
        public string MethodName { get; set; }

        [JsonProperty("params")]
        public object Parameters { get; set; }

        public static Request FromJson(string json) =>
            JsonConvert.DeserializeObject<Request>(json);

        public string ToJson() =>
            JsonConvert.SerializeObject(
                this, JsonSerializerSettingsSource.Settings);

    }
}
