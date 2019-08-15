using RPCExp.Modbus;
using Newtonsoft.Json;
using RPCExp.Common;
using System.Collections;

namespace RPCExp.Modbus.Factory
{
    public static class Factory
    {
        static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            // TODO ! Избегать такого при помощи сервисов-ресурсов.
            // PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.Indented,
        };

        public static void SaveFacility(Facility obj, string file = "cfg.json")
        {
            var dcw = new FacilityCfgWrapper();
            dcw.Wrap(obj);
            var cfg = JsonConvert.SerializeObject(dcw, jsonSerializerSettings);
            System.IO.File.WriteAllText(file, cfg);
        }

        public static Facility LoadFacility(string file)
        {
            var cfg = System.IO.File.ReadAllText(file);
            return JsonConvert.DeserializeObject<FacilityCfgWrapper>(cfg, jsonSerializerSettings)?.Unwrap();
        }
    }

}
