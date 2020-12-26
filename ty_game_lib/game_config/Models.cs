using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace game_config
{
    [Serializable]
    public class skill : IGameConfig
    {
        public string id { get; set; }
        public int AmmoCost { get; set; }
        public int SnipeStepNeed { get; set; }
        public int BaseTough { get; set; }
        public string LockArea { get; set; }
        public Dictionary<float, string> LaunchTimeToBullet { get; set; }
        public bool CanInputMove { get; set; }
        public Point[] Moves { get; set; }
        public float MoveStartTime { get; set; }
        public float BreakSnipeTime { get; set; }
        public float SkillMustTime { get; set; }
        public float ComboInputStartTime { get; set; }
        public float SkillMaxTime { get; set; }
        public int NextCombo { get; set; }
    }

    [Serializable]
    public class bad_words : IGameConfig
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    [Serializable]
    public class caught_buff : IGameConfig
    {
        public string id { get; set; }
        public float LastTime { get; set; }
        public SimpleObj1[] CatchKeyPoints { get; set; }
        public string TrickSkill { get; set; }
    }

    [Serializable]
    public class prop : IGameConfig
    {
        public int id { get; set; }
        public string Info { get; set; }
        public int PropPointCost { get; set; }
        public int UseCond { get; set; }
        public int BaseTough { get; set; }
        public Dictionary<float, Media> LaunchTimeToEffectM { get; set; }
        public float PropMustTime { get; set; }
        public float MoveSpeedMulti { get; set; }
        public float MustMoveSpeedMulti { get; set; }
        public float TurnLimit { get; set; }
    }

    [Serializable]
    public class talent : IGameConfig
    {
        public int id { get; set; }
        public int passive_id { get; set; }
        public int activeLevel { get; set; }
        public Cost[] activeCost { get; set; }
        public int addLevel { get; set; }
        public Cost addLevelBaseCost { get; set; }
        public Cost addLevelAddCost { get; set; }
    }

    [Serializable]
    public class other_config : IGameConfig
    {
        public int id { get; set; }
        public float max_hegiht { get; set; }
        public float g_acc { get; set; }
        public float friction { get; set; }
        public int tough_grow { get; set; }
        public int mid_tough { get; set; }
        public int weapon_num { get; set; }
        public float two_s_to_see_pertick { get; set; }
        public int qspace_max_per_level { get; set; }
        public uint hit_wall_add_tick_by_speed_param { get; set; }
        public int hit_wall_dmg_param { get; set; }
        public uint hit_wall_catch_tick_param { get; set; }
        public float hit_wall_catch_dmg_param { get; set; }
        public float sight_length { get; set; }
        public float sight_width { get; set; }
        public string common_fail_antibuff { get; set; }
        public int tick_per_sec { get; set; }
        public float interaction_act2_call_time { get; set; }
        public int trick_protect_value { get; set; }
        public float protect_time { get; set; }
        public int standard_max_prop_stack { get; set; }
        public int standard_recycle_prop_stack { get; set; }
        public float prop_R { get; set; }
        public float weapon_R { get; set; }
        public float pass_R { get; set; }
        public float saleBox_R { get; set; }
        public int up_trap_max { get; set; }
        public float rogue_reborn_limit_time { get; set; }
        public float ShardedAttackMulti { get; set; }
        public float DecreaseMinCos { get; set; }
        public float NormalSpeedMinCos { get; set; }
        public float MoveDecreaseMinMulti { get; set; }
        public Gain[] rogue_reborn_cost { get; set; }
    }

    [Serializable]
    public class trap : IGameConfig
    {
        public string id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public size BodyId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public base_attr_id AttrId { get; set; }

        public bool CanBeSee { get; set; }
        public int FailChance { get; set; }
        public float CallTrapRoundTime { get; set; }
        public float MaxLifeTime { get; set; }
        public Media TrapMedia { get; set; }
        public Media[] LauchMedia { get; set; }
        public float TrickDelayTime { get; set; }
        public uint TrickStack { get; set; }
        public float DamageMulti { get; set; }
    }

    [Serializable]
    public class radar_wave : IGameConfig
    {
        public string id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public raw_shape ShapeType { get; set; }

        public float[] ShapeParams { get; set; }
        public Point LocalPos { get; set; }
        public int LocalRotate { get; set; }
    }

    [Serializable]
    public class creep : IGameConfig
    {
        public int id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public size BodyId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public base_attr_id AttrId { get; set; }

        public float DestoryDelayTime { get; set; }
        public string DestoryBullet { get; set; }
        public int[] Weapons { get; set; }
        public int MaxWeaponSlot { get; set; }
        public string OutActSkill { get; set; }
        public Point VScope { get; set; }
    }

    [Serializable]
    public class snipe : IGameConfig
    {
        public int id { get; set; }
        public int TrickTick { get; set; }
        public int TotalStep { get; set; }
        public int OnTickSpeed { get; set; }
        public int OffTickSpeed { get; set; }
        public float MoveMulti { get; set; }
    }

    [Serializable]
    public class body : IGameConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public size id { get; set; }

        public float mass { get; set; }
        public float rad { get; set; }
    }

    [Serializable]
    public class skill_group : IGameConfig
    {
        public string id { get; set; }
        public Dictionary<int, string> Op1 { get; set; }
        public Dictionary<int, string> Op2 { get; set; }
        public Dictionary<int, string> Op3 { get; set; }
        public Dictionary<int, string> Switch { get; set; }
    }

    [Serializable]
    public class interaction : IGameConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public interactionAct id { get; set; }

        public int BaseTough { get; set; }
        public float TotalTime { get; set; }
    }

    [Serializable]
    public class show_text : IGameConfig
    {
        public string id { get; set; }
        public string chinese { get; set; }
    }

    [Serializable]
    public class bullet : IGameConfig
    {
        public string id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public raw_shape ShapeType { get; set; }

        public float[] ShapeParams { get; set; }
        public Point LocalPos { get; set; }
        public int LocalRotate { get; set; }
        public Buff[] SuccessAntiActBuffConfigToOpponent { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public hit_type HitType { get; set; }

        public Buff[] FailActBuffConfigToSelf { get; set; }
        public int PauseToCaster { get; set; }
        public int PauseToOpponent { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public target_type TargetType { get; set; }

        public int Tough { get; set; }
        public int ProtectValue { get; set; }
        public int SuccessAmmoAdd { get; set; }
        public float DamageMulti { get; set; }
    }

    [Serializable]
    public class character : IGameConfig
    {
        public int id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public size BodyId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public base_attr_id AttrId { get; set; }

        public int[] Weapons { get; set; }
        public int MaxWeaponSlot { get; set; }
    }

    [Serializable]
    public class push_buff : IGameConfig
    {
        public string id { get; set; }
        public float LastTime { get; set; }
        public int BuffType { get; set; }
        public float PushForce { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PushType PushType { get; set; }

        public Point[] FixVector { get; set; }
        public float UpForce { get; set; }
    }

    [Serializable]
    public class fish : IGameConfig
    {
        public int id { get; set; }
        public string name_text { get; set; }
        public string name { get; set; }
        public int fish_type { get; set; }
        public int aim_type { get; set; }
        public string fish_show_type { get; set; }
        public string fish_show_sub_type { get; set; }
        public string fish_show_type_text { get; set; }
        public string fish_show_sub_type_text { get; set; }
        public string multi_show_string { get; set; }
        public string multi_show_string_text { get; set; }
        public bool yellow_BG { get; set; }
        public string res_pack { get; set; }
        public float scale { get; set; }
        public SimpleObj2[] ring_pos { get; set; }
        public Point anchor_multi { get; set; }
        public float animation_play_time_span { get; set; }
        public int deep { get; set; }
        public string boss_res { get; set; }
        public int appear_frame { get; set; }
        public int move_frame { get; set; }
        public int die_frame { get; set; }
        public int turning_mode { get; set; }
        public float appear_time { get; set; }
        public bool is_3d_model { get; set; }
        public SimpleObj3 param_for_3D { get; set; }
        public string[] dead_sound_list { get; set; }
        public string appear_sound { get; set; }
        public string change_bgm { get; set; }
        public float bgm_delay { get; set; }
        public float appear_loop { get; set; }
        public int special_fish_type { get; set; }
        public string show_game { get; set; }
        public SimpleObj5 rectangle_collider_anchor { get; set; }
        public SimpleObj6 rectangle_collider { get; set; }
        public int spawn_rate { get; set; }
        public int size { get; set; }
        public int common_spawn_gap { get; set; }
        public float z_axle_width { get; set; }
        public int bonus_multi { get; set; }
        public float yiwangdajin_scale { get; set; }
        public int kill_show_effect { get; set; }
        public float night_pearl_rate { get; set; }
        public float night_pearl_multi { get; set; }
        public bool is_trigger_lottery { get; set; }
        public int diamond_num { get; set; }
        public int coin_num { get; set; }
        public bool is_capture_broadcast { get; set; }
        public int exp { get; set; }
    }

    [Serializable]
    public class summon : IGameConfig
    {
        public string id { get; set; }
        public string Setter { get; set; }
    }

    [Serializable]
    public class vehicle : IGameConfig
    {
        public int id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public size BodyId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public base_attr_id AttrId { get; set; }

        public float DestoryDelayTime { get; set; }
        public string DestoryBullet { get; set; }
        public int[] Weapons { get; set; }
        public int MaxWeaponSlot { get; set; }
        public string OutActSkill { get; set; }
        public Point VScope { get; set; }
    }

    [Serializable]
    public class self_effect : IGameConfig
    {
        public string id { get; set; }
        public float HealMulti { get; set; }
        public float FixMulti { get; set; }
        public float ShieldMulti { get; set; }
        public float ReloadMulti { get; set; }
        public int[] AddPlayBuffs { get; set; }
    }

    [Serializable]
    public class lock_area : IGameConfig
    {
        public string id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public raw_shape ShapeType { get; set; }

        public float[] ShapeParams { get; set; }
        public Point LocalPos { get; set; }
        public int LocalRotate { get; set; }
    }

    [Serializable]
    public class item : IGameConfig
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string another_name { get; set; }
        public string Icon { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ItemType ItemType { get; set; }

        public bool IsPlayingItem { get; set; }
        public bool IsEndClear { get; set; }
        public uint MaxStack { get; set; }
        public int ShowType { get; set; }
    }

    [Serializable]
    public class weapon : IGameConfig
    {
        public int id { get; set; }
        public SimpleObj7[] BodySizeUseAndSnipeSpeedFix { get; set; }
        public Dictionary<int, float> BotRange { get; set; }
        public float MaxRangeMulti { get; set; }
        public int ChangeRangeStep { get; set; }
        public int Snipe1 { get; set; }
        public int Snipe2 { get; set; }
        public int Snipe3 { get; set; }
    }

    [Serializable]
    public class passive : IGameConfig
    {
        public int id { get; set; }
        public string info { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public passive_type passive_effect_type { get; set; }

        public Gain[] recycle_money { get; set; }
        public float[] param_values { get; set; }
    }

    [Serializable]
    public class base_attribute : IGameConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public base_attr_id id { get; set; }

        public float MoveMaxSpeed { get; set; }
        public float MoveAddSpeed { get; set; }
        public float MoveMinSpeed { get; set; }
        public float RecycleMulti { get; set; }
        public uint Atk { get; set; }
        public uint ShardedNum { get; set; }
        public int MaxAmmo { get; set; }
        public float ReloadMulti { get; set; }
        public float BackStabAdd { get; set; }
        public uint MaxHP { get; set; }
        public float HealEffect { get; set; }
        public uint MaxArmor { get; set; }
        public uint ArmorDefence { get; set; }
        public float ArmorFixEffect { get; set; }
        public uint MaxShield { get; set; }
        public float ShieldDelayTime { get; set; }
        public float ShieldChargeEffect { get; set; }
        public uint ShieldRecover { get; set; }
        public uint ShieldInstability { get; set; }
        public uint MaxTrapNum { get; set; }
        public float TrapAtkMulti { get; set; }
        public float TrapSurvivalMulti { get; set; }
        public float HPAbsorb { get; set; }
        public float ArmorAbsorb { get; set; }
        public float ShieldAbsorb { get; set; }
        public float AmmoAbsorb { get; set; }
        public float ProtectAbsorb { get; set; }
    }

    [Serializable]
    public class standard_level_up : IGameConfig
    {
        public int id { get; set; }
        public int next_exp { get; set; }
        public float reborn_time { get; set; }
        public int[] up_passive_get { get; set; }
        public Gain[] fastReborn { get; set; }
    }

    [Serializable]
    public enum ItemType
    {
        @bag,
        @money
    }

    [Serializable]
    public enum effect_media_type
    {
        @bullet,
        @radar_wave,
        @summon,
        @self
    }

    [Serializable]
    public enum size
    {
        @medium,
        @small,
        @big,
        @tiny,
        @default
    }

    [Serializable]
    public enum base_attr_id
    {
        @armor_mech,
        @mine_attr,
        @fresh_mech,
        @shield_mech,
        @standard_body,
        @alert_attr,
        @creep_body_fresh
    }

    [Serializable]
    public enum raw_shape
    {
        @sector,
        @rectangle,
        @round
    }

    [Serializable]
    public enum interactionAct
    {
        @kick_vehicle,
        @apply,
        @recycle_cage,
        @get_in_vehicle,
        @pick_up_cage,
        @get_info
    }

    [Serializable]
    public enum buff_type
    {
        @caught_buff,
        @push_buff
    }

    [Serializable]
    public enum hit_type
    {
        @melee,
        @range
    }

    [Serializable]
    public enum target_type
    {
        @other_team,
        @all_team
    }

    [Serializable]
    public enum PushType
    {
        @Vector,
        @Center
    }

    [Serializable]
    public enum passive_type
    {
        @AbsorbAdd,
        @TrapAbout,
        @AddItem,
        @Survive,
        @Special,
        @Other,
        @TickAdd,
        @Attack,
        @Regen
    }

    public static class ResNames
    {
        public static Dictionary<Type, string> NamesDictionary { get; } = new Dictionary<Type, string>
        {
            {typeof(skill), "skill_s.json"}, {typeof(bad_words), "bad_words_s.json"},
            {typeof(caught_buff), "caught_buff_s.json"}, {typeof(prop), "prop_s.json"},
            {typeof(talent), "talent_s.json"}, {typeof(other_config), "other_config_s.json"},
            {typeof(trap), "trap_s.json"}, {typeof(radar_wave), "radar_wave_s.json"}, {typeof(creep), "creep_s.json"},
            {typeof(snipe), "snipe_s.json"}, {typeof(body), "body_s.json"}, {typeof(skill_group), "skill_group_s.json"},
            {typeof(interaction), "interaction_s.json"}, {typeof(show_text), "show_text_s.json"},
            {typeof(bullet), "bullet_s.json"}, {typeof(character), "character_s.json"},
            {typeof(push_buff), "push_buff_s.json"}, {typeof(fish), "fish_s.json"}, {typeof(summon), "summon_s.json"},
            {typeof(vehicle), "vehicle_s.json"}, {typeof(self_effect), "self_effect_s.json"},
            {typeof(lock_area), "lock_area_s.json"}, {typeof(item), "item_s.json"}, {typeof(weapon), "weapon_s.json"},
            {typeof(passive), "passive_s.json"}, {typeof(base_attribute), "base_attribute_s.json"},
            {typeof(standard_level_up), "standard_level_up_s.json"}
        };

        public static string[] Names { get; } =
        {
            "skill", "bad_words", "caught_buff", "prop", "talent", "other_config", "trap", "radar_wave", "creep",
            "snipe", "body", "skill_group", "interaction", "show_text", "bullet", "character", "push_buff", "fish",
            "summon", "vehicle", "self_effect", "lock_area", "item", "weapon", "passive", "base_attribute",
            "standard_level_up"
        };
    }

    [Serializable]
    public class ConfigDictionaries
    {
        public ImmutableDictionary<string, skill> skills { get; set; }
        public ImmutableDictionary<int, bad_words> bad_wordss { get; set; }
        public ImmutableDictionary<string?, caught_buff> caught_buffs { get; set; }
        public ImmutableDictionary<int, prop> props { get; set; }
        public ImmutableDictionary<int, talent> talents { get; set; }
        public ImmutableDictionary<int, other_config> other_configs { get; set; }
        public ImmutableDictionary<string, trap> traps { get; set; }
        public ImmutableDictionary<string, radar_wave> radar_waves { get; set; }
        public ImmutableDictionary<int, creep> creeps { get; set; }
        public ImmutableDictionary<int, snipe> snipes { get; set; }
        public ImmutableDictionary<size, body> bodys { get; set; }
        public ImmutableDictionary<string, skill_group> skill_groups { get; set; }
        public ImmutableDictionary<interactionAct, interaction> interactions { get; set; }
        public ImmutableDictionary<string, show_text> show_texts { get; set; }
        public ImmutableDictionary<string, bullet> bullets { get; set; }
        public ImmutableDictionary<int, character> characters { get; set; }
        public ImmutableDictionary<string, push_buff> push_buffs { get; set; }
        public ImmutableDictionary<int, fish> fishs { get; set; }
        public ImmutableDictionary<string, summon> summons { get; set; }
        public ImmutableDictionary<int, vehicle> vehicles { get; set; }
        public ImmutableDictionary<string, self_effect> self_effects { get; set; }
        public ImmutableDictionary<string, lock_area> lock_areas { get; set; }
        public ImmutableDictionary<int, item> items { get; set; }
        public ImmutableDictionary<int, weapon> weapons { get; set; }
        public ImmutableDictionary<int, passive> passives { get; set; }
        public ImmutableDictionary<base_attr_id, base_attribute> base_attributes { get; set; }
        public ImmutableDictionary<int, standard_level_up> standard_level_ups { get; set; }
        public IDictionary[] all_Immutable_dictionary { get; set; }
#if NETCOREAPP
        public ConfigDictionaries()
        {
            LoadAllByDll();


            all_Immutable_dictionary = new IDictionary[]
            {
                skills, bad_wordss, caught_buffs, props, talents, other_configs, traps, radar_waves, creeps, snipes,
                bodys, skill_groups, interactions, show_texts, bullets, characters, push_buffs, fishs, summons,
                vehicles, self_effects, lock_areas, items, weapons, passives, base_attributes, standard_level_ups
            };
        }
#endif

        public ConfigDictionaries(string jsonPath = "")
        {
            LoadAllByJson(jsonPath);


            all_Immutable_dictionary = new IDictionary[]
            {
                skills, bad_wordss, caught_buffs, props, talents, other_configs, traps, radar_waves, creeps, snipes,
                bodys, skill_groups, interactions, show_texts, bullets, characters, push_buffs, fishs, summons,
                vehicles, self_effects, lock_areas, items, weapons, passives, base_attributes, standard_level_ups
            };
        }

        public ConfigDictionaries(Dictionary<string, string> nameToJsonString)
        {
            LoadAllByJsonString(nameToJsonString);

            all_Immutable_dictionary = new IDictionary[]
            {
                skills, bad_wordss, caught_buffs, props, talents, other_configs, traps, radar_waves, creeps, snipes,
                bodys, skill_groups, interactions, show_texts, bullets, characters, push_buffs, fishs, summons,
                vehicles, self_effects, lock_areas, items, weapons, passives, base_attributes, standard_level_ups
            };
        }

#if NETCOREAPP
        public void LoadAllByDll()
        {
            skills = GameConfigTools.GenConfigDict<string, skill>();
            bad_wordss =
                GameConfigTools.GenConfigDict<int, bad_words>();
            caught_buffs =
                GameConfigTools.GenConfigDict<string, caught_buff>();
            props = GameConfigTools.GenConfigDict<int, prop>();
            talents =
                GameConfigTools.GenConfigDict<int, talent>();
            other_configs = GameConfigTools.GenConfigDict<int, other_config>();
            traps
                = GameConfigTools.GenConfigDict<string, trap>();
            radar_waves =
                GameConfigTools.GenConfigDict<string, radar_wave>();
            creeps = GameConfigTools.GenConfigDict<int, creep>();
            snipes =
                GameConfigTools.GenConfigDict<int, snipe>();
            bodys = GameConfigTools.GenConfigDict<size, body>();
            skill_groups =
                GameConfigTools.GenConfigDict<string, skill_group>();
            interactions =
                GameConfigTools.GenConfigDict<interactionAct, interaction>();
            show_texts =
                GameConfigTools.GenConfigDict<string, show_text>();
            bullets =
                GameConfigTools.GenConfigDict<string, bullet>();
            characters =
                GameConfigTools.GenConfigDict<int, character>();
            push_buffs = GameConfigTools.GenConfigDict<string, push_buff>();
            fishs
                = GameConfigTools.GenConfigDict<int, fish>();
            summons = GameConfigTools.GenConfigDict<string, summon>();
            vehicles =
                GameConfigTools.GenConfigDict<int, vehicle>();
            self_effects =
                GameConfigTools.GenConfigDict<string, self_effect>();
            lock_areas =
                GameConfigTools.GenConfigDict<string, lock_area>();
            items = GameConfigTools.GenConfigDict<int, item>();
            weapons =
                GameConfigTools.GenConfigDict<int, weapon>();
            passives = GameConfigTools.GenConfigDict<int, passive>();
            base_attributes
                = GameConfigTools.GenConfigDict<base_attr_id, base_attribute>();
            standard_level_ups =
                GameConfigTools.GenConfigDict<int, standard_level_up>();
        }
#endif
        public void LoadAllByJson(string path = "")
        {
            skills = GameConfigTools.GenConfigDictByJsonFile<string, skill>(path);
            bad_wordss = GameConfigTools.GenConfigDictByJsonFile<int, bad_words>(path);
            caught_buffs = GameConfigTools.GenConfigDictByJsonFile<string, caught_buff>(path);
            props = GameConfigTools.GenConfigDictByJsonFile<int, prop>(path);
            talents = GameConfigTools.GenConfigDictByJsonFile<int, talent>(path);
            other_configs = GameConfigTools.GenConfigDictByJsonFile<int, other_config>(path);
            traps = GameConfigTools.GenConfigDictByJsonFile<string, trap>(path);
            radar_waves = GameConfigTools.GenConfigDictByJsonFile<string, radar_wave>(path);
            creeps = GameConfigTools.GenConfigDictByJsonFile<int, creep>(path);
            snipes = GameConfigTools.GenConfigDictByJsonFile<int, snipe>(path);
            bodys = GameConfigTools.GenConfigDictByJsonFile<size, body>(path);
            skill_groups = GameConfigTools.GenConfigDictByJsonFile<string, skill_group>(path);
            interactions = GameConfigTools.GenConfigDictByJsonFile<interactionAct, interaction>(path);
            show_texts = GameConfigTools.GenConfigDictByJsonFile<string, show_text>(path);
            bullets = GameConfigTools.GenConfigDictByJsonFile<string, bullet>(path);
            characters = GameConfigTools.GenConfigDictByJsonFile<int, character>(path);
            push_buffs = GameConfigTools.GenConfigDictByJsonFile<string, push_buff>(path);
            fishs = GameConfigTools.GenConfigDictByJsonFile<int, fish>(path);
            summons = GameConfigTools.GenConfigDictByJsonFile<string, summon>(path);
            vehicles = GameConfigTools.GenConfigDictByJsonFile<int, vehicle>(path);
            self_effects = GameConfigTools.GenConfigDictByJsonFile<string, self_effect>(path);
            lock_areas = GameConfigTools.GenConfigDictByJsonFile<string, lock_area>(path);
            items = GameConfigTools.GenConfigDictByJsonFile<int, item>(path);
            weapons = GameConfigTools.GenConfigDictByJsonFile<int, weapon>(path);
            passives = GameConfigTools.GenConfigDictByJsonFile<int, passive>(path);
            base_attributes = GameConfigTools.GenConfigDictByJsonFile<base_attr_id, base_attribute>(path);
            standard_level_ups = GameConfigTools.GenConfigDictByJsonFile<int, standard_level_up>(path);
        }

        public void LoadAllByJsonString(Dictionary<string, string> nameToJsonString)
        {
            skills = GameConfigTools.GenConfigDictByJsonString<string, skill>(nameToJsonString["skill"]);
            bad_wordss = GameConfigTools.GenConfigDictByJsonString<int, bad_words>(nameToJsonString["bad_words"]);
            caught_buffs =
                GameConfigTools.GenConfigDictByJsonString<string, caught_buff>(nameToJsonString["caught_buff"]);
            props = GameConfigTools.GenConfigDictByJsonString<int, prop>(nameToJsonString["prop"]);
            talents = GameConfigTools.GenConfigDictByJsonString<int, talent>(nameToJsonString["talent"]);
            other_configs =
                GameConfigTools.GenConfigDictByJsonString<int, other_config>(nameToJsonString["other_config"]);
            traps = GameConfigTools.GenConfigDictByJsonString<string, trap>(nameToJsonString["trap"]);
            radar_waves = GameConfigTools.GenConfigDictByJsonString<string, radar_wave>(nameToJsonString["radar_wave"]);
            creeps = GameConfigTools.GenConfigDictByJsonString<int, creep>(nameToJsonString["creep"]);
            snipes = GameConfigTools.GenConfigDictByJsonString<int, snipe>(nameToJsonString["snipe"]);
            bodys = GameConfigTools.GenConfigDictByJsonString<size, body>(nameToJsonString["body"]);
            skill_groups =
                GameConfigTools.GenConfigDictByJsonString<string, skill_group>(nameToJsonString["skill_group"]);
            interactions =
                GameConfigTools.GenConfigDictByJsonString<interactionAct, interaction>(nameToJsonString["interaction"]);
            show_texts = GameConfigTools.GenConfigDictByJsonString<string, show_text>(nameToJsonString["show_text"]);
            bullets = GameConfigTools.GenConfigDictByJsonString<string, bullet>(nameToJsonString["bullet"]);
            characters = GameConfigTools.GenConfigDictByJsonString<int, character>(nameToJsonString["character"]);
            push_buffs = GameConfigTools.GenConfigDictByJsonString<string, push_buff>(nameToJsonString["push_buff"]);
            fishs = GameConfigTools.GenConfigDictByJsonString<int, fish>(nameToJsonString["fish"]);
            summons = GameConfigTools.GenConfigDictByJsonString<string, summon>(nameToJsonString["summon"]);
            vehicles = GameConfigTools.GenConfigDictByJsonString<int, vehicle>(nameToJsonString["vehicle"]);
            self_effects =
                GameConfigTools.GenConfigDictByJsonString<string, self_effect>(nameToJsonString["self_effect"]);
            lock_areas = GameConfigTools.GenConfigDictByJsonString<string, lock_area>(nameToJsonString["lock_area"]);
            items = GameConfigTools.GenConfigDictByJsonString<int, item>(nameToJsonString["item"]);
            weapons = GameConfigTools.GenConfigDictByJsonString<int, weapon>(nameToJsonString["weapon"]);
            passives = GameConfigTools.GenConfigDictByJsonString<int, passive>(nameToJsonString["passive"]);
            base_attributes =
                GameConfigTools.GenConfigDictByJsonString<base_attr_id, base_attribute>(
                    nameToJsonString["base_attribute"]);
            standard_level_ups =
                GameConfigTools.GenConfigDictByJsonString<int, standard_level_up>(
                    nameToJsonString["standard_level_up"]);
        }
    }

    [Serializable]
    public class Point : IGameConfig
    {
        public float x { get; set; }
        public float y { get; set; }
    }

    [Serializable]
    public class SimpleObj1 : IGameConfig
    {
        public float key_time { get; set; }
        public Point key_point { get; set; }
    }

    [Serializable]
    public class Media : IGameConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public effect_media_type media_type { get; set; }

        public string e_id { get; set; }
    }

    [Serializable]
    public class Cost : IGameConfig
    {
        public int item { get; set; }
        public int num { get; set; }
        public int first_use_pay { get; set; }
    }

    [Serializable]
    public class Gain : IGameConfig
    {
        public int item { get; set; }
        public int num { get; set; }
    }

    [Serializable]
    public class Buff : IGameConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public size size { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public buff_type buff_type { get; set; }

        public string buff_id { get; set; }
    }

    [Serializable]
    public class SimpleObj2 : IGameConfig
    {
        public int x { get; set; }
        public int y { get; set; }
        public float scale { get; set; }
    }

    [Serializable]
    public class SimpleObj4 : IGameConfig
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }

    [Serializable]
    public class SimpleObj3 : IGameConfig
    {
        public SimpleObj4 pos { get; set; }
        public SimpleObj4 rotate { get; set; }
        public float scale { get; set; }
    }

    [Serializable]
    public class SimpleObj5 : IGameConfig
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    [Serializable]
    public class SimpleObj6 : IGameConfig
    {
        public int width { get; set; }
        public int height { get; set; }
    }

    [Serializable]
    public class SimpleObj7 : IGameConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public size body { get; set; }

        public float snipe_speed_fix { get; set; }
        public string skill_group { get; set; }
    }
}