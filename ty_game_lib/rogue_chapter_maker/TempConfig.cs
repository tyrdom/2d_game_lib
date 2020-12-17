using System.Collections.Generic;
using game_config;

namespace rogue_chapter_maker
{
    public static class TempConfig
    {
        public static ConfigDictionaries Configs { get; private set; }
            = GameConfigTools.Configs;

        private static void ReLoadP(ConfigDictionaries configs)
        {
            Configs = configs;
            var configsOtherConfig = configs.other_configs[1];
        }
#if NETCOREAPP
        public static void LoadConfig()
        {
            var configs = new ConfigDictionaries();
            ReLoadP(configs);
        }
#else
        public static void LoadConfig(Dictionary<string, string> jsons)
        {
            var configs = new ConfigDictionaries(jsons);
            ReLoadP(configs);
        }
#endif
    }
}