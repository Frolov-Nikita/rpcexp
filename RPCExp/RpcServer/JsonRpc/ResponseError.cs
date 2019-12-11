using Newtonsoft.Json;

namespace RPCExp.RpcServer.JsonRpc
{
    public class ResponseError
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public object Data { get; set; }

        public static Response FromJson(string json) =>
            JsonConvert.DeserializeObject<Response>(json);

        public string ToJson()
        {
            var jsonData = Data == null ? "" : ",\"data\":" + JsonConvert.SerializeObject(Data, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return $"{{\"code\":{Code}, \"message\":{JsonConvert.SerializeObject(Message)}{jsonData}}}";
        }
    }//class ResponseError
}
