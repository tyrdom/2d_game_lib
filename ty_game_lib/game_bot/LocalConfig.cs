using System.Collections.Immutable;
using cov_path_navi;
using game_config;

namespace game_bot
{
    public class LocalConfig
    {
        public static float PatrolSlowMulti { get; private set; } = 0.5f;

        public static float CloseEnoughDistance { get; private set; } = 0.2f;

        public static float PatrolMin { get; private set; } = 0.5f;
        public static float PatrolMax { get; private set; } = 0.8f;

        public static ImmutableDictionary<int, ImmutableDictionary<size, PathTop>> NaviMapPerLoad { get; private set; }
            =
            game_stuff.LocalConfig.PerLoadMapConfig.ToImmutableDictionary(p => p.Key,
                p => p.Value.WalkMap.SizeToEdge.ToImmutableDictionary(pp => pp.Key, pp => new PathTop(pp.Value)));

        public static void ReLoad()
        {
            NaviMapPerLoad =
                game_stuff.LocalConfig.PerLoadMapConfig.ToImmutableDictionary(p => p.Key,
                    p => p.Value.WalkMap.SizeToEdge.ToImmutableDictionary(pp => pp.Key, pp => new PathTop(pp.Value)));
        }
    }
}