using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public static class TempConfig
    {
        public static ConfigDictionaries configs { get; set; } = new ConfigDictionaries();

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

        public static readonly float G = configs.other_configs[1].g_acc;
        public static readonly float MaxHeight = configs.other_configs[1].max_hegiht;
        public static readonly float MaxUpSpeed = MathTools.Sqrt(2f * G * MaxHeight);

        public static readonly float Friction = configs.other_configs[1].friction;

        public static readonly int ToughGrowPerTick = configs.other_configs[1].tough_grow;
        public static readonly int MidTough = configs.other_configs[1].mid_tough;
        public static int WeaponNum = configs.other_configs[1].weapon_num;
        public static readonly float TwoSToSeePerTick = configs.other_configs[1].two_s_to_see_pertick;
        public static readonly PushOnAir OutCaught = new PushOnAir(new TwoDVector(0, 0), 0.05f, 0, 6);

        public static readonly int QSpaceBodyMaxPerLevel = configs.other_configs[1].qspace_max_per_level;

        public static readonly int HitWallTickParam = configs.other_configs[1].hit_wall_add_tick_by_speed_param;
        public static readonly int HitWallCatchTickParam = configs.other_configs[1].hit_wall_catch_tick_param;
        public static readonly int HitWallDmgParam = configs.other_configs[1].hit_wall_dmg_param;
        public static readonly float HitWallCatchDmgParam = configs.other_configs[1].hit_wall_catch_dmg_param;

        public static readonly float StandardSightR = configs.other_configs[1].standard_sight_r;

        public static readonly TwoDVector StandardSightVector =
            new TwoDVector(configs.other_configs[1].sight_length, configs.other_configs[1].sight_width);

        public static readonly IAntiActBuffConfig CommonBuffConfig =
            GameTools.GenBuffByConfig(configs.push_buffs[configs.other_configs[1].common_fail_antibuff]);

        public const int StartHp = 1000;
        public static int TestAtk = 10;

        public static int TrickProtect = 100;
        public static int ProtectTick = 10;
    }
}