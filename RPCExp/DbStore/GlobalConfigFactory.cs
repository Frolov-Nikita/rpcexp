using Newtonsoft.Json;
using System.IO;

namespace RPCExp.DbStore
{
    /// <summary>
    /// Source of global config.
    /// It knows how to (re)store it.
    /// </summary>
    public static class GlobalConfigFactory
    {
        private static GlobalConfig globalConfig;
        private static string fileName;

        /// <summary>
        /// gets cached config or loads it if cache is empty.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Loads cache from storage
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static GlobalConfig Load(string fileName)
        {
            var cfgStr = File.ReadAllText(fileName);
            var cfg = JsonConvert.DeserializeObject<GlobalConfig>(cfgStr);
            return cfg;
        }

        /// <summary>
        /// Saves config into storage
        /// </summary>
        /// <param name="config"></param>
        /// <param name="fileName"></param>
        public static void Save(GlobalConfig config, string fileName)
        {
            var cfgStr = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(fileName, cfgStr);
        }
    }
}
