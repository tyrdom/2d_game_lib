using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game_config;
using game_stuff;
using rogue_chapter_maker;

namespace rogue_game
{
    public static class LocalConfig
    {
       
        public static bool CanEndRest(int itemId)
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

        public static int GameRuleCheckTick { get; private set; } = 10;
        public static GameItem[] RogueRebornCost { get; private set; } = { };
        public static int RogueRebornTick { get; private set; } = 1000;
        private static void ReLoadP(ConfigDictionaries configs)
        {
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