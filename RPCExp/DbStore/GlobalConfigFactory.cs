using Newtonsoft.Json;
using System.IO;

namespace RPCExp.DbStore
{
    public static class GlobalConfigFactory
    {
        private static GlobalConfig globalConfig;
        private static string fileName;

        public static GlobalConfig Get(string file = "RPCExpGlobalCfg.json")
        {
            if ((fileName == file) && (globalConfig != default))
                return globalConfig;

            fileName = file;

            if (File.Exists(fileName))
            {
                globalConfig = Load(fileName);
            }
            else
            {
                globalConfig = new GlobalConfig();
                Save(globalConfig, fileName);
            }

            return globalConfig;
        }

        public static GlobalConfig Load(string fileName)
        {
            var cfgStr = File.ReadAllText(fileName);
            var cfg = JsonConvert.DeserializeObject<GlobalConfig>(cfgStr);
            return cfg;
        }

        public static void Save(GlobalConfig config, string fileName)
        {
            var cfgStr = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(fileName, cfgStr);
        }
    }
}
