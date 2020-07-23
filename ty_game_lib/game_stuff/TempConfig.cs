using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public static class TempConfig
    {
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

        public static readonly float G = Content.other_configs[1].g_acc;
        public static readonly float MaxHeight = Content.other_configs[1].max_hegiht;
        public static readonly float MaxUpSpeed = MathTools.Sqrt(2f * G * MaxHeight);

        public static readonly float Friction = Content.other_configs[1].friction;

        public static readonly int ToughGrowPerTick = Content.other_configs[1].tough_grow;
        public static readonly int MidTough = Content.other_configs[1].mid_tough;
        public static int WeaponNum = Content.other_configs[1].weapon_num;
        public static readonly float TwoSToSeePerTick = Content.other_configs[1].two_s_to_see_pertick;
        public static readonly PushOnAir OutCaught = new PushOnAir(new TwoDVector(0, 0), 0.05f, 0, 6);

        public static readonly int QSpaceBodyMaxPerLevel = Content.other_configs[1].qspace_max_per_level;

        public static readonly int HitWallTickParam = Content.other_configs[1].hit_wall_add_tick_by_speed_param;
        public static readonly int HitWallCatchTickParam = Content.other_configs[1].hit_wall_catch_tick_param;
        public static readonly int HitWallDmgParam = Content.other_configs[1].hit_wall_dmg_param;
        public static readonly float HitWallCatchDmgParam = Content.other_configs[1].hit_wall_catch_dmg_param;

        public static float StandardSightR = Content.other_configs[1].standard_sight_r;

        public static TwoDVector StandardVector =
            new TwoDVector(Content.other_configs[1].sight_length, Content.other_configs[1].sight_width);

        public const int StartHp = 1000;
        public static int TestAtk = 10;

        public static int TrickProtect = 100;
        public static int ProtectTick = 10;
    }
}