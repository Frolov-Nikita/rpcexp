using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RPCExp.RpcServer.JsonRpc
{

    public class Response
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("jsonrpc")]
        public string Version { get; set; } = "2.0";

        [JsonProperty("result")]
        public object Result { get; set; } = null;

        [JsonProperty("error")]
        public ResponseError Error { get; set; } = null;

        public static Response FromJson(string json) => 
            JsonConvert.DeserializeObject<Response>(json);

        public string ToJson()
        {
            var jsonId = "\"id\":" + (Id == default ? "null": "\"" + Id +"\"");
            if (Error == null)
                //return $"{{\"jsonrpc\":\"{Version}\",{jsonId},\"result\":{JsonConvert.SerializeObject(Result, Formatting.Indented, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})}}}";
                return $"{{\"jsonrpc\":\"{Version}\",{jsonId},\"result\":{JsonConvert.SerializeObject(Result, JsonSerializerSettingsSource.Settings)}}}";
            else
                return $"{{\"jsonrpc\":\"{Version}\",{jsonId},\"error\":{Error.ToJson()}}}";
        }

        /*
        code             |	message     	    |   meaning
        -----------------+----------------------+----------------------
        -32700           |	Parse error 	    |   Invalid JSON was received by the server. An error occurred on the server while parsing the JSON text.
        -32600           |	Invalid Request	    |   The JSON sent is not a valid Request object.
        -32601           |	Method not found	|   The method does not exist / is not available.
        -32602           |	Invalid params  	|   Invalid method parameter(s).
        -32603           |	Internal error  	|   Internal JSON-RPC error.
        -32000 to -32099 | Server error	        |   Reserved for implementation-defined server-errors.
         */

        public static Response GetErrorParse(string id = "") => 
            new Response {
                Id = id,
                Error = new ResponseError {
                    Code = -32700,
                    Message = "Parse error"
                }
            };

        public static Response GetErrorInvalidRequest()=> new Response
            {
                Error = new ResponseError
                {
                    Code = -32600,
                    Message = "Invalid Request"
                }
        };
        

        public static Response GetErrorMethodNotFound(string id, string methodName) => new Response
        {
            Id = id,
            Error = new ResponseError
            {
                Code = -32601,
                Message = $"Method \"{methodName}\" not found",
            }
        };

        public static Response GetErrorInvalidParams(string id, string methodName, object paramerers) => new Response
        {
            Id = id,
            Error = new ResponseError
            {
                Code = -32602,
                Message = $"Invalid params in \"{methodName}\"",
                Data = paramerers,
            }
        };

        public static Response GetErrorInternalError(string id, string methodName, string message="") => new Response
        {
            Id = id,
            Error = new ResponseError
            {
                Code = -32602,
                Message = $"Internal error \"{methodName}\". {message}",
            }
        };
    }
}
