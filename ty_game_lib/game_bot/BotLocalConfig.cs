using System.Collections.Generic;
using System.Collections.Immutable;
using cov_path_navi;
using game_config;

namespace game_bot
{
    public static class BotLocalConfig
    {
        public static float PatrolSlowMulti { get; private set; } = 0.5f;

        public static float CloseEnoughDistance { get; private set; } = 0.25f;

        public static float PatrolMin { get; private set; } = 0.5f;
        public static float PatrolMax { get; private set; } = 0.8f;

        public static float KeepDistance { get; private set; } = 6f;
        public static float MaxTraceDistance { get; set; } = 8f;

        public static ImmutableDictionary<int, ImmutableDictionary<size, PathTop>> NaviMapPerLoad { get; private set; }
            =
            game_stuff.StuffLocalConfig.PerLoadMapConfig.ToImmutableDictionary(p => p.Key,
                p => p.Value.WalkMap.SizeToEdge.ToImmutableDictionary(pp => pp.Key, pp => new PathTop(pp.Value)));

        public static void ReLoadP(ConfigDictionaries configDictionaries)
        {
            game_stuff.StuffLocalConfig.ReLoadP(configDictionaries);
            NaviMapPerLoad =
                game_stuff.StuffLocalConfig.PerLoadMapConfig.ToImmutableDictionary(p => p.Key,
                    p => p.Value.WalkMap.SizeToEdge.ToImmutableDictionary(pp => pp.Key, pp => new PathTop(pp.Value)));
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