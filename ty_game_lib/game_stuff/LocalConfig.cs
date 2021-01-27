using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public static class LocalConfig
    {
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


        public static float G { get; private set; } = 1;

        public static float MaxHeight { get; private set; } = 2;

        public static float MaxUpSpeed { get; private set; } = MathTools.Sqrt(2f * G * MaxHeight);

        public static float Friction { get; private set; } = 0.24f;

        public static int ToughGrowPerTick { get; private set; } = 100;

        public static int MidTough { get; private set; } = 1000;

        public static int StandardWeaponNum { get; private set; } = 2;

        public static float TwoSToSeePerTick { get; private set; } = 50f;

        public static float TwoSToSeePerTickInMidV { get; private set; } = 70f;

        public static PushOnAir OutCaught(IBattleUnitStatus caster)
        {
            return new PushOnAir(new TwoDVector(0, 0), 0.05f, 0, 6, caster);
        }

        public static int QSpaceBodyMaxPerLevel { get; private set; } = 5;

        public static uint HitWallTickParam { get; private set; } = 10;

        public static uint HitWallCatchTickParam { get; private set; } = 5;
        public static int HitWallDmgParam { get; private set; } = 10;
        public static float HitWallCatchDmgParam { get; private set; } = 5f;


        public static TwoDVector StandardSightVector { get; private set; } =
            new TwoDVector(10, 7);

        public static IStunBuffMaker CommonBuffMaker { get; private set; } =
            new PushEarthStunBuffMaker(1f, PushType.Center, null, 12);


        public static float ShardedAttackMulti { get; private set; } = 0.05f;


        public static int TrickProtect { get; private set; } = 100;
        public static int ProtectTick { get; private set; } = 10;
        public static float MoveDecreaseMinMulti { get; private set; } = 0.4f;

        public static float NormalSpeedMinCos { get; private set; } = 0.7f;

        public static float DecreaseMinCos { get; private set; } = -0.3f;


        public static int StandardPropMaxStack { get; private set; } = 100;

        public static int StandardPropRecycleStack { get; private set; } = 30;
        public static float PropR { get; private set; } = 1f;

        public static float WeaponR { get; private set; } = 1f;


        public static float PassiveR { get; private set; } = 1f;
        public static float SaleBoxR { get; private set; } = 1f;
        public static uint MaxCallActTwoTick { get; private set; } = 10;


        public static int UpMaxTrap { get; private set; } = 10;
        public static int AtkPassBuffId { get; private set; } = 0;
        public static int DefPassBuffId { get; private set; } = 0;

        public static void ReLoadP(ConfigDictionaries configs)
        {
            CommonConfig.ReLoadP(configs);
            var configsOtherConfig = CommonConfig.Configs.other_configs[1];
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
            CommonBuffMaker =
                StunBuffStandard.GenBuffByConfig(configs.push_buffs[configsOtherConfig.common_fail_antibuff]);
            ShardedAttackMulti = configsOtherConfig.ShardedAttackMulti;
            MaxCallActTwoTick = CommonConfig.GetTickByTime(configsOtherConfig.interaction_act2_call_time);
            TrickProtect = configsOtherConfig.trick_protect_value;
            ProtectTick = (int) CommonConfig.GetTickByTime(configsOtherConfig.protect_time);
            StandardPropMaxStack = configsOtherConfig.standard_max_prop_stack;
            StandardPropRecycleStack = configsOtherConfig.standard_recycle_prop_stack;
            PropR = configsOtherConfig.prop_R;
            WeaponR = configsOtherConfig.weapon_R;
            PassiveR = configsOtherConfig.pass_R;
            SaleBoxR = configsOtherConfig.saleBox_R;
            UpMaxTrap = configsOtherConfig.up_trap_max;
            MoveDecreaseMinMulti = configsOtherConfig.MoveDecreaseMinMulti;
            NormalSpeedMinCos = configsOtherConfig.NormalSpeedMinCos;
            DecreaseMinCos = configsOtherConfig.DecreaseMinCos;
            AtkPassBuffId = configsOtherConfig.atkPassBuffId;
            DefPassBuffId = configsOtherConfig.defPassBuffId;
            TwoSToSeePerTickInMidV = configsOtherConfig.two_s_to_see_pertick_medium_vehicle;
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