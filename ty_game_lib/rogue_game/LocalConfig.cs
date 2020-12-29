using System.Collections.Generic;
using System.IO;
using System.Linq;
using game_config;
using game_stuff;

namespace rogue_game
{
    public static class LocalConfig
    {
        public static ConfigDictionaries Configs { get; private set; }
            = CommonConfig.Configs;


        public static bool CanEndRest(int itemId)
        {
            if (Configs.items.TryGetValue(itemId, out var item))
            {
                return !item.IsEndClear;
            }

            throw new DirectoryNotFoundException($"not such item id{itemId}");
        }

        public static int GameRuleCheckTick { get; private set; } = 10;
        public static GameItem[] RogueRebornCost { get; private set; } = { };
        public static int RogueRebornTick { get; private set; } = 1000;
        private static void ReLoadP(ConfigDictionaries configs)
        {
            game_stuff.LocalConfig.ReLoadP(configs);
            Configs = CommonConfig.Configs;
            var configsOtherConfig = configs.other_configs[1];
            RogueRebornCost = configsOtherConfig.rogue_reborn_cost.Select(GameItem.GenByConfigGain).ToArray();
            RogueRebornTick = (int) CommonConfig.GetTickByTime(configsOtherConfig.rogue_reborn_limit_time);
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