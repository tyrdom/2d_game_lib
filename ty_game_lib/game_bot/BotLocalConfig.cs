using System.Collections.Generic;
using System.Collections.Immutable;
using cov_path_navi;
using game_config;

namespace game_bot
{
    public static class BotLocalConfig
    {
        public static float PatrolSlowMulti { get; private set; } = 0.5f;
        public static float CloseEnoughDistance { get; private set; } = 0.5f;
        public static float PatrolMin { get; private set; } = 0.5f;
        public static float PatrolMax { get; private set; } = 0.8f;
        public static float MaxTraceDistance { get; private set; } = 16f;
        public static int LockTraceTickTime { get; private set; } = 50;

        public static ImmutableDictionary<int, ImmutableDictionary<size, PathTop>> NaviMapPerLoad { get; private set; }
            =
            game_stuff.StuffLocalConfig.PerLoadMapConfig.ToImmutableDictionary(p => p.Key,
                p => p.Value.WalkMap.SizeToEdge
                    .ToImmutableDictionary(pp => pp.Key, pp => new PathTop(pp.Value)));

        public static void ReLoadP()
        {
            NaviMapPerLoad =
                game_stuff.StuffLocalConfig.PerLoadMapConfig.ToImmutableDictionary(p => p.Key,
                    p => p.Value.WalkMap.SizeToEdge.ToImmutableDictionary(pp => pp.Key, pp => new PathTop(pp.Value)));

            var configsBotOtherConfig = CommonConfig.Configs.bot_other_configs[1];
            PatrolSlowMulti = configsBotOtherConfig.PatrolSlowMulti;
            CloseEnoughDistance = configsBotOtherConfig.CloseEnoughDistance;
            PatrolMin = configsBotOtherConfig.PatrolMin;
            PatrolMax = configsBotOtherConfig.PatrolMax;
            MaxTraceDistance = configsBotOtherConfig.MaxTraceDistance;
            LockTraceTickTime = (int) configsBotOtherConfig.LockTraceTickTime;
        }
    }
}