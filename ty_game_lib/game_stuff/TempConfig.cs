using System;
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
            [BodySize.Small] = 0.5f,
            [BodySize.Medium] = 2f,
            [BodySize.Big] = 3f
        };

        public static readonly Dictionary<BodySize, float> SizeToMass = new Dictionary<BodySize, float>
        {
            [BodySize.Small] = 1f,
            [BodySize.Medium] = 4f,
            [BodySize.Big] = 9f
        };

        public static float G { get; private set; } = 1;

        public static float MaxHeight { get; private set; } = 2;

        public static float MaxUpSpeed { get; private set; } = MathTools.Sqrt(2f * G * MaxHeight);

        public static float Friction { get; private set; } = 0.24f;

        public static int ToughGrowPerTick { get; private set; } = 100;

        public static int MidTough { get; private set; } = 1000;

        public static int WeaponNum { get; private set; } = 2;

        public static float TwoSToSeePerTick { get; private set; } = 20f;

        public static PushOnAir OutCaught { get;  } = new PushOnAir(new TwoDVector(0, 0), 0.05f, 0, 6);

        public static int QSpaceBodyMaxPerLevel { get; private set; } = 5;

        public static int HitWallTickParam { get; private set; } = 10;

        public static int HitWallCatchTickParam { get; private set; } = 5;
        public static int HitWallDmgParam { get; private set; } = 10;
        public static float HitWallCatchDmgParam { get; private set; } = 5f;


        public static TwoDVector StandardSightVector { get; private set; } =
            new TwoDVector(10, 7);

        public static IAntiActBuffConfig CommonBuffConfig { get; private set; } =
            new PushEarthAntiActBuffConfig(1f, PushType.Center, null, 12);

        public static int StartHp { get; private set; } = 1000;
        private static int TestAtk { get; set; } = 10;

        private static int TrickProtect { get; set; } = 100;
        private static int ProtectTick { get; set; } = 10;


        public static float MoveDecreaseMinMulti { get; set; } = 0.3f;

        public static float NormalSpeedMinCos { get; set; } = 0.7f;

        public static float DecreaseMinCos { get; set; } = -0.3f;

        private static void ReLoadP(ConfigDictionaries configs)
        {
            Configs = configs;
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
    }
}