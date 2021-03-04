using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public static class LocalConfig
    {
        public static bool PloyListCheckOk(this IEnumerable<Poly> list)
        {
            var enumerable = list.ToList();
            var any = enumerable.Any(x => enumerable.Any(p => p != x && p.CrossAnotherOne(x)));
            return !any;
        }

        public static Poly GenPoly(this IEnumerable<Point> points)
        {
            return new Poly(points.Select(x => new TwoDPoint(x.x, x.y)).ToArray());
        }

        public static List<(Poly poly, bool isBlockIn)> PloyListMark(this IEnumerable<Poly> polys)
        {
            var list = polys.IeToHashSet();

            if (!list.Any())
            {
                return new List<(Poly poly, bool isBlockIn)>();
            }


            var valueTuples = new List<(Poly poly, bool isBlockIn)>();
            const bool nowBlockIn = false;

            static void Mark(List<(Poly poly, bool isBlockIn)> collector, ISet<Poly> resList, bool nowIsBlockIn)
            {
                if (!resList.Any())
                {
                    return;
                }

                var enumerable = resList.Where(x => !resList.Any(p => p != x && p.IsIncludeAnother(x))).ToArray();
                collector.AddRange(enumerable.Select(x => (x, nowIsBlockIn)));
                resList.ExceptWith(enumerable);
                Mark(collector, resList, !nowIsBlockIn);
            }

            Mark(valueTuples, list, nowBlockIn);

#if DEBUG
            valueTuples.Aggregate("", ((s, tuple) => s +
                                                     tuple.poly + tuple.isBlockIn));
            Console.Out.WriteLine("");
#endif
            return valueTuples;
        }

        public static IHitMedia GenHitMedia(Media media)
        {
            var effectMedia = GenMedia(media);
            if (effectMedia is IHitMedia hitMedia)
                return hitMedia;
            throw new ArgumentOutOfRangeException($"not a hit media {media.e_id}");
        }

        public static IEffectMedia GenMedia(Media media)
        {
            var aId = media.e_id;
            return media.media_type switch
            {
                effect_media_type.summon => Summon.GenById(aId),
                effect_media_type.self => SelfEffect.GenById(aId),
                effect_media_type.radar_wave => RadarWave.GenById(aId),
                effect_media_type.bullet => Bullet.GenById(aId),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static float GetRBySize(size bodySize)
        {
            return CommonConfig.Configs.bodys[bodySize].rad;
        }

        public static ImmutableDictionary<int, MapInitData> PerLoadMapConfig { get; private set; } =
            new Dictionary<int, MapInitData>().ToImmutableDictionary();

        public static ImmutableDictionary<int, ImmutableDictionary<direction, TwoDPoint[]>> PerLoadMapTransPort
        {
            get;
            private set;
        } =
            new Dictionary<int, ImmutableDictionary<direction, TwoDPoint[]>>().ToImmutableDictionary();


        public static float MaxUpSpeed { get; private set; } =
            MathTools.Sqrt(2f * CommonConfig.OtherConfig.g_acc * CommonConfig.OtherConfig.max_hegiht);

        public static PushOnAir OutCaught(IBattleUnitStatus caster)
        {
            return new PushOnAir(new TwoDVector(0, 0), 0.05f, 0, 6, caster);
        }

        public static uint HitWallTickParam { get; private set; } = 2;
        public static uint HitWallCatchTickParam { get; private set; } = 5;


        public static TwoDVector StandardSightVector { get; private set; } =
            new TwoDVector(10, 7);

        public static IStunBuffMaker CommonBuffMaker { get; private set; } =
            new PushEarthStunBuffMaker(1f, PushType.Center, null, 12);


        public static int ProtectTick { get; private set; } = 10;


        public static uint MaxCallActTwoTick { get; private set; } = 10;


        public static void ReLoadP(ConfigDictionaries configDictionaries)
        {
            CommonConfig.ReLoadP(configDictionaries);
            PerLoadMapTransPort = CommonConfig.Configs.map_rawss.ToImmutableDictionary(p => p.Key,
                p => p.Value.TransPoint.GroupBy(obj2 => obj2.Direction)
                    .ToImmutableDictionary(
                        s => s.Key,
                        s => s
                            .SelectMany(ss => ss.Teleport
                                .Select(pt => new TwoDPoint(pt.x, pt.y)))
                            .ToArray()));

            PerLoadMapConfig = CommonConfig.Configs.map_rawss.ToImmutableDictionary(x => x.Key,
                x => PlayGround.GenEmptyByConfig(x.Value));

            var configsOtherConfig = CommonConfig.OtherConfig;
            MaxUpSpeed = MathTools.Sqrt(2f * CommonConfig.OtherConfig.g_acc * CommonConfig.OtherConfig.max_hegiht);
            HitWallTickParam = CommonConfig.GetTickByTime(configsOtherConfig.hit_wall_add_time_by_speed_param);
            HitWallCatchTickParam = CommonConfig.GetTickByTime(configsOtherConfig.hit_wall_catch_time_param);
            StandardSightVector =
                new TwoDVector(configsOtherConfig.sight_length, configsOtherConfig.sight_width);
            CommonBuffMaker =
                StunBuffStandard.GenBuffByConfig(
                    CommonConfig.Configs.push_buffs[configsOtherConfig.common_fail_antibuff]);
            MaxCallActTwoTick = CommonConfig.GetTickByTime(configsOtherConfig.interaction_act2_call_time);
            ProtectTick = CommonConfig.GetIntTickByTime(configsOtherConfig.protect_time);
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