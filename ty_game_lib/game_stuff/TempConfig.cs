﻿using System;
using System.Collections.Generic;
using System.Reflection;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public static class TempConfig
    {
        public static ConfigDictionaries Configs { get; set; }
#if NETCOREAPP
            = new ConfigDictionaries();
#else
            = new ConfigDictionaries("");
#endif

        public static readonly Dictionary<BodySize, float> SizeToR = new Dictionary<BodySize, float>
        {
            [BodySize.Small] = 1.5f,
            [BodySize.Medium] = 3f,
            [BodySize.Big] = 4.5f
        };

        public static readonly Dictionary<BodySize, float> SizeToMass = new Dictionary<BodySize, float>
        {
            [BodySize.Small] = 2.25f,
            [BodySize.Medium] = 9f,
            [BodySize.Big] = 20.25f
        };

        public static float G = 1;

        public static float MaxHeight { get; private set; } = 2;

        public static float MaxUpSpeed { get; private set; } = MathTools.Sqrt(2f * G * MaxHeight);

        public static float Friction { get; private set; } = 1f;

        public static int ToughGrowPerTick { get; private set; } = 100;

        public static int MidTough { get; private set; } = 1000;

        public static int WeaponNum { get; private set; } = 2;

        public static float TwoSToSeePerTick { get; private set; } = 20f;

        public static PushOnAir OutCaught { get; set; } = new PushOnAir(new TwoDVector(0, 0), 0.05f, 0, 6);

        public static int QSpaceBodyMaxPerLevel { get; private set; } = 5;

        public static int HitWallTickParam { get; private set; } = 10;

        public static int HitWallCatchTickParam { get; private set; } = 5;
        public static int HitWallDmgParam { get; private set; } = 10;
        public static float HitWallCatchDmgParam { get; private set; } = 5f;


        public static TwoDVector StandardSightVector { get; private set; } =
            new TwoDVector(15, 10);

        public static IAntiActBuffConfig CommonBuffConfig { get; private set; } =
            new PushEarthAntiActBuffConfig(1f, PushType.Center, null, 12);

        public static int StartHp { get; private set; } = 1000;
        private static int TestAtk { get; set; } = 10;

        private static int TrickProtect { get; set; } = 100;
        private static int ProtectTick { get; set; } = 10;

        private static void ReLoadP(ConfigDictionaries configs)
        {
            G = configs.other_configs[1].g_acc;

            MaxHeight = configs.other_configs[1].max_hegiht;
            MaxUpSpeed = MathTools.Sqrt(2f * G * MaxHeight);

            Friction = configs.other_configs[1].friction;

            ToughGrowPerTick = configs.other_configs[1].tough_grow;
            MidTough = configs.other_configs[1].mid_tough;
            WeaponNum = configs.other_configs[1].weapon_num;
            TwoSToSeePerTick = configs.other_configs[1].two_s_to_see_pertick;

            QSpaceBodyMaxPerLevel = configs.other_configs[1].qspace_max_per_level;

            HitWallTickParam = configs.other_configs[1].hit_wall_add_tick_by_speed_param;
            HitWallCatchTickParam = configs.other_configs[1].hit_wall_catch_tick_param;
            HitWallDmgParam = configs.other_configs[1].hit_wall_dmg_param;
            HitWallCatchDmgParam = configs.other_configs[1].hit_wall_catch_dmg_param;

            StandardSightVector =
                new TwoDVector(configs.other_configs[1].sight_length, configs.other_configs[1].sight_width);

            CommonBuffConfig =
                GameTools.GenBuffByConfig(configs.push_buffs[configs.other_configs[1].common_fail_antibuff]);

            StartHp = 1000;
            TestAtk = 10;

            TrickProtect = 100;
            ProtectTick = 10;
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
        
        // public MapInitData TestInitData(){}
        public static WalkMap TestMap()
        {
            var pt1 = new TwoDPoint(0.0f, 0.0f);
            var pt2 = new TwoDPoint(1.0f, 1.0f);
            var pt3 = new TwoDPoint(2f, 0f);
            var pt4 = new TwoDPoint(3.0f, 1f);
            var pt5 = new TwoDPoint(4.0f, 0f);

            var pt6 = new TwoDPoint(2.0f, -2.0f);
            var twoDPoints = new[] {pt1, pt2, pt3, pt4, pt5, pt6};
            var poly = new Poly(twoDPoints);
            var tuples = new List<(Poly, bool)>
            {
                (poly, false)
            };
            var mapByPolys = WalkMap.CreateMapByPolys(tuples);
            return mapByPolys;
        }


        public static SightMap TestSightMap()
        {
            var pt1 = new TwoDPoint(0.0f, 0.0f);
            var pt2 = new TwoDPoint(1.0f, 1.0f);
            var pt3 = new TwoDPoint(2f, 0f);
            var pt4 = new TwoDPoint(3.0f, 1f);
            var pt5 = new TwoDPoint(4.0f, 0f);

            var pt6 = new TwoDPoint(2.0f, -2.0f);
            var twoDPoints = new[] {pt1, pt2, pt3, pt4, pt5, pt6};
            var poly = new Poly(twoDPoints);
            var tuples = new List<(Poly, bool)>
            {
                (poly, false)
            };
            var mapByPolys = SightMap.GenByConfig(tuples, new TwoDVectorLine[] { });
            return mapByPolys;
        }
        
        
        
    }
}