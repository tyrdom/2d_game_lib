using System.Collections.Generic;

namespace game_config
{
    public static class CommonConfig
    {
        public static ConfigDictionaries Configs { get; private set; }
#if NETCOREAPP
            = new ConfigDictionaries();
#else
            = new ConfigDictionaries("");
#endif
        public static uint GetTickByTime(float time)
        {
            return (uint) (time * TickPerSec);
        }

        public static int GetIntTickByTime(float time)
        {
            return (int) (time * TickPerSec);
        }

        public static float NumPerSecToTickPerSec(float numPerSec)
        {
            return numPerSec / TickPerSec;
        }

        public static uint NumPerSecToTickPerSec(uint numPerSec)
        {
            return (uint) (numPerSec / TickPerSec);
        }

        public static int NumPerSecToTickPerSec(int numPerSec)
        {
            return numPerSec / TickPerSec;
        }


        private static int TickPerSec { get; set; } = 10;
        public static void ReLoadP(ConfigDictionaries configs)
        {
            Configs = configs;
            var configsOtherConfig = configs.other_configs[1];
            TickPerSec = configsOtherConfig.tick_per_sec;
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