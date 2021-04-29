using System;
using System.Collections.Generic;
using System.Linq;
using game_config;
using game_stuff;
using rogue_chapter_maker;

namespace rogue_game
{
    public static class RogueLocalConfig
    {
        public static int GetRuleCheckIntTickByTime(float time)
        {
            return (int) (time / CommonConfig.OtherConfig.rogueGameCheckTickTime);
        }

        public static int ConsoleTickAsGameTickNum()
        {
            return (int) (CommonConfig.OtherConfig.rogueGameCheckTickTime / CommonConfig.OtherConfig.tick_time);
        }

        public static bool CanEndRest(item_id itemId)
        {
            if (CommonConfig.Configs.items.TryGetValue(itemId, out var item))
            {
                return !item.IsEndClear;
            }

            throw new KeyNotFoundException($"not such item id{itemId}");
        }

        public static size[] AllowSizes(this MapType mapType)
        {
            return mapType switch
            {
                MapType.BigStart => new[] {size.medium, size.small},
                MapType.BigEnd => new[] {size.medium, size.small},
                MapType.Small => new[] {size.small},
                MapType.Big => new[] {size.medium, size.small},
                MapType.SmallStart => new[] {size.small},
                MapType.SmallEnd => new[] {size.small},
                MapType.Vendor => new[] {size.small},
                MapType.Hangar => new[] {size.medium, size.small},
                _ => throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null)
            };
        }

        public static direction OppositeDirection(this direction direction)
        {
            return direction switch
            {
                direction.West => direction.East,
                direction.South => direction.North,
                direction.East => direction.West,
                direction.North => direction.South,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        public static int GameRuleCheckTickPerSec { get; private set; } = 1;
        public static GameItem[] RogueRebornCost { get; private set; } = { };
        public static int RogueRebornTick { get; private set; } = 1000;
        public static void ReLoadP(ConfigDictionaries configs)
        {
            game_bot.BotLocalConfig.ReLoadP(configs);
            var configsOtherConfig = CommonConfig.OtherConfig;
            RogueRebornCost = configsOtherConfig.rogue_reborn_cost.Select(GameItem.GenByConfigGain).ToArray();
            RogueRebornTick = GetRuleCheckIntTickByTime(configsOtherConfig.rogue_reborn_limit_time);
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