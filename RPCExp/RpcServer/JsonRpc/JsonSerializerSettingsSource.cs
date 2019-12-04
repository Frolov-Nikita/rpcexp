using Newtonsoft.Json;

namespace RPCExp.RpcServer.JsonRpc
{
    internal static class JsonSerializerSettingsSource
    {
        private static JsonSerializerSettings settings;
        public static JsonSerializerSettings Settings
        {
            get
            {
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
