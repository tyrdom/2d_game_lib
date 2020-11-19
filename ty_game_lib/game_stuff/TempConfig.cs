using System.Collections.Generic;
using System.Collections.Immutable;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public static class TempConfig
    {
        public static float GetRBySize(BodySize bodySize)
        {
            return SizeToR[bodySize];
        }

        public static uint GetTickByTime(float time)
        {
            return (uint) (time * TickPerSec);
        }

        public static float NumSecToTick(float numPerSec)
        {
            return numPerSec / TickPerSec;
        }

        public static uint NumSecToTick(uint numPerSec)
        {
            return (uint) (numPerSec / TickPerSec);
        }

        public static int NumSecToTick(int numPerSec)
        {
            return numPerSec / TickPerSec;
        }

        public static ConfigDictionaries Configs { get; private set; }
#if NETCOREAPP
            = new ConfigDictionaries();
#else
            = new ConfigDictionaries("");
#endif

        public static readonly Dictionary<BodySize, float> SizeToR = new Dictionary<BodySize, float>
        {
            [BodySize.Small] = 0.5f,
            [BodySize.Medium] = 1f,
            [BodySize.Big] = 1.5f
        };

        public static readonly Dictionary<BodySize, float> SizeToMass = new Dictionary<BodySize, float>
        {
            [BodySize.Small] = 1f,
            [BodySize.Medium] = 4f,
            [BodySize.Big] = 9f
        };


        public static ImmutableDictionary<uint, IPlayingBuffConfig> BuffConfigs { get; set; } =
            ImmutableDictionary<uint, IPlayingBuffConfig>.Empty;

        public static float G { get; private set; } = 1;

        public static float MaxHeight { get; private set; } = 2;

        public static float MaxUpSpeed { get; private set; } = MathTools.Sqrt(2f * G * MaxHeight);

        public static float Friction { get; private set; } = 0.24f;

        public static int ToughGrowPerTick { get; private set; } = 100;

        public static int MidTough { get; private set; } = 1000;

        public static int StandardWeaponNum { get; private set; } = 2;

        public static float TwoSToSeePerTick { get; private set; } = 20f;

        public static PushOnAir OutCaught { get; } = new PushOnAir(new TwoDVector(0, 0), 0.05f, 0, 6);

        public static int QSpaceBodyMaxPerLevel { get; private set; } = 5;

        public static uint HitWallTickParam { get; private set; } = 10;

        public static uint HitWallCatchTickParam { get; private set; } = 5;
        public static int HitWallDmgParam { get; private set; } = 10;
        public static float HitWallCatchDmgParam { get; private set; } = 5f;


        public static TwoDVector StandardSightVector { get; private set; } =
            new TwoDVector(10, 7);

        public static IAntiActBuffConfig CommonBuffConfig { get; private set; } =
            new PushEarthAntiActBuffConfig(1f, PushType.Center, null, 12);


        public static float ShardedAttackMulti { get; private set; } = 0.05f;


        public static int TrickProtect { get; private set; } = 100;
        public static int ProtectTick { get; private set; } = 10;
        private static int TickPerSec { get; set; } = 10;
        public static float MoveDecreaseMinMulti { get; set; } = 0.4f;

        public static float NormalSpeedMinCos { get; set; } = 0.7f;

        public static float DecreaseMinCos { get; set; } = -0.3f;


        public static int StandardPropMaxStack { get; private set; } = 100;

        public static float PropR { get; private set; } = 1f;

        public static float WeaponR { get; private set; } = 1f;


        public static float PassiveR { get; private set; } = 1f;
        public static float SaleBoxR { get; private set; } = 1f;
        public static uint MaxCallActTwoTick { get; private set; } = 10;
        private static void ReLoadP(ConfigDictionaries configs)
        {
            Configs = configs;
            var configsOtherConfig = configs.other_configs[1];
            G = configsOtherConfig.g_acc;
            MaxHeight = configsOtherConfig.max_hegiht;
            MaxUpSpeed = MathTools.Sqrt(2f * G * MaxHeight);
            Friction = configsOtherConfig.friction;
            ToughGrowPerTick = configsOtherConfig.tough_grow;
            MidTough = configsOtherConfig.mid_tough;
            StandardWeaponNum = configsOtherConfig.weapon_num;
            TwoSToSeePerTick = configsOtherConfig.two_s_to_see_pertick;
            QSpaceBodyMaxPerLevel = configsOtherConfig.qspace_max_per_level;
            HitWallTickParam = configsOtherConfig.hit_wall_add_tick_by_speed_param;
            HitWallCatchTickParam = configsOtherConfig.hit_wall_catch_tick_param;
            HitWallDmgParam = configsOtherConfig.hit_wall_dmg_param;
            HitWallCatchDmgParam = configsOtherConfig.hit_wall_catch_dmg_param;
            StandardSightVector =
                new TwoDVector(configsOtherConfig.sight_length, configsOtherConfig.sight_width);
            CommonBuffConfig =
                GameTools.GenBuffByConfig(configs.push_buffs[configsOtherConfig.common_fail_antibuff]);

            MaxCallActTwoTick = GetTickByTime(configsOtherConfig.interaction_act2_call_time);
            TrickProtect = configsOtherConfig.trick_protect_value;
            ProtectTick = (int) GetTickByTime(configsOtherConfig.protect_time);
            StandardPropMaxStack = configsOtherConfig.standard_max_prop_stack;

            PropR = configsOtherConfig.prop_R;
            WeaponR = configsOtherConfig.weapon_R;
            PassiveR = configsOtherConfig.pass_R;
            SaleBoxR = configsOtherConfig.saleBox_R;
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