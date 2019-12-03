using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.RpcServer.JsonRpc
{
    static class JsonSerializerSettingsSource
    {

        static JsonSerializerSettings settings;
        public static JsonSerializerSettings Settings
        {
            get {
                if (settings == default)
                {
                    settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented,
                    };
                    settings.Converters.Add(new DecimalJsonConverter());
                }

                return settings;
            }
        }
    }
}
