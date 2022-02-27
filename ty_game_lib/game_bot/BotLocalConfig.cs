using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using cov_path_navi;
using game_config;
using game_stuff;

namespace game_bot
{
    public static class BotLocalConfig
    {
        public static bot_other_config BotOtherConfig { get; private set; } = null!;


        public static ImmutableDictionary<int, ImmutableDictionary<size, PathTop>>
            NaviMapPerLoad { get; private set; } = null!;


        public static void ReLoadP()
        {
            BotOtherConfig = CommonConfig.Configs.bot_other_configs[1];
#if DEBUG
            Console.Out.WriteLine($"Gen Poly For Navi {BotOtherConfig.NaviPloyRadMulti}  {BotOtherConfig.NaviPathGoThroughMulti} ");
#endif
            NaviMapPerLoad = ToImmutableDictionary(CommonConfig.Configs.map_rawss);
        }

        private static ImmutableDictionary<int, ImmutableDictionary<size, PathTop>> ToImmutableDictionary(
            Dictionary<int, map_raws> configsMapRawsS)
        {
            return configsMapRawsS.ToImmutableDictionary(p => p.Key, p =>
            {
                var enumerable = p.Value.WalkRawMap.Select(x => x.GenPoly()).ToArray();
                var walkMap = enumerable.Any()
                    ? enumerable.PloyListCheckOk()
                        ? WalkMap.CreateMapByPolys(enumerable.PloyListMark(), BotOtherConfig.NaviPloyRadMulti)
                        : throw new Exception($"no good walk raw poly in {p.Key} ")
                    : throw new Exception("must have walk map");
                return walkMap.SizeToEdge.ToImmutableDictionary(pp => pp.Key, pp => new PathTop(pp.Value));
            });
        }
    }
}