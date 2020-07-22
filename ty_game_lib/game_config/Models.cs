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
        public int BaseTough { get; set; }
        public string LockArea { get; set; }
        public Dictionary<uint, string> LaunchTickToBullet { get; set; }
        public Point[] Moves { get; set; }
        public int MoveStartTick { get; set; }
        public string LockAreaBox { get; set; }
        public int HomingStartTick { get; set; }
        public int HomingEndTick { get; set; }
        public int SkillMustTick { get; set; }
        public int ComboInputStartTick { get; set; }
        public int SkillMaxTick { get; set; }
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
        public int LastTick { get; set; }
        public Point[] CatchPoints { get; set; }
        public string TrickSkill { get; set; }
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
        public int hit_wall_add_tick_by_speed_param { get; set; }
        public int hit_wall_dmg_param { get; set; }
        public int hit_wall_catch_tick_param { get; set; }
        public float hit_wall_catch_dmg_param { get; set; }
        public float standard_sight_r { get; set; }
        public float sight_length { get; set; }
        public float sight_width { get; set; }
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
    public class show_text : IGameConfig
    {
        public string id { get; set; }
        public string chinese { get; set; }
    }

    [Serializable]
    public class bullet : IGameConfig
    {
        public string id { get; set; }
        public int ShapeType { get; set; }
        public float[] ShapeParams { get; set; }
        public Point LocalPos { get; set; }
        public int LocalRotate { get; set; }
        public string SuccessAntiActBuffConfigToOpponent { get; set; }
        public Dictionary<string, Buff> FailActBuffConfigToSelf { get; set; }
        public int PauseToCaster { get; set; }
        public int PauseToOpponent { get; set; }
        public int TargetType { get; set; }
        public int Tough { get; set; }
    }

    [Serializable]
    public enum push_buff_PushType
    {
        @Center,
        @Vector
    }

    [Serializable]
    public class push_buff : IGameConfig
    {
        public string id { get; set; }
        public int LastTick { get; set; }
        public int BuffType { get; set; }
        public float PushForce { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public push_buff_PushType PushType { get; set; }

        public Point[] FixVector { get; set; }
        public float UpForce { get; set; }
    }

    [Serializable]
    public class lock_area : IGameConfig
    {
        public string id { get; set; }
        public int ShapeType { get; set; }
        public float[] ShapeParams { get; set; }
        public Point LocalPos { get; set; }
        public int LocalRotate { get; set; }
    }

    [Serializable]
    public class item : IGameConfig
    {
        public int id { get; set; }
        public bool IsMoney { get; set; }
        public int ShowType { get; set; }
        public string Name { get; set; }
        public string another_name { get; set; }
        public string Icon { get; set; }
    }

    [Serializable]
    public class weapon : IGameConfig
    {
        public string id { get; set; }
        public Dictionary<int, string> Op1 { get; set; }
        public Dictionary<int, string> Op2 { get; set; }
        public Dictionary<int, string> Op3 { get; set; }
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
        public SimpleObj1[] ring_pos { get; set; }
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
        public SimpleObj2 param_for_3D { get; set; }
        public string[] dead_sound_list { get; set; }
        public string appear_sound { get; set; }
        public string change_bgm { get; set; }
        public float bgm_delay { get; set; }
        public float appear_loop { get; set; }
        public int special_fish_type { get; set; }
        public string show_game { get; set; }
        public SimpleObj4 rectangle_collider_anchor { get; set; }
        public SimpleObj5 rectangle_collider { get; set; }
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
    public enum size
    {
        @medium,
        @big,
        @small
    }

    public static class ResNames
    {
        public static Dictionary<Type, string> NamesDictionary = new Dictionary<Type, string>
        {
            {typeof(skill), "skill_s.json"}, {typeof(bad_words), "bad_words_s.json"},
            {typeof(caught_buff), "caught_buff_s.json"}, {typeof(other_config), "other_config_s.json"},
            {typeof(body), "body_s.json"}, {typeof(show_text), "show_text_s.json"}, {typeof(bullet), "bullet_s.json"},
            {typeof(push_buff), "push_buff_s.json"}, {typeof(lock_area), "lock_area_s.json"},
            {typeof(item), "item_s.json"}, {typeof(weapon), "weapon_s.json"}, {typeof(fish), "fish_s.json"}
        };
    }

    [Serializable]
    public static class Content
    {
        public static ImmutableDictionary<string, skill> skills { get; set; } =
            GameConfigTools.GenConfigDict<string, skill>();

        public static ImmutableDictionary<int, bad_words> bad_wordss { get; set; } =
            GameConfigTools.GenConfigDict<int, bad_words>();

        public static ImmutableDictionary<string, caught_buff> caught_buffs { get; set; } =
            GameConfigTools.GenConfigDict<string, caught_buff>();

        public static ImmutableDictionary<int, other_config> other_configs { get; set; } =
            GameConfigTools.GenConfigDict<int, other_config>();

        public static ImmutableDictionary<size, body> bodys { get; set; } = GameConfigTools.GenConfigDict<size, body>();

        public static ImmutableDictionary<string, show_text> show_texts { get; set; } =
            GameConfigTools.GenConfigDict<string, show_text>();

        public static ImmutableDictionary<string, bullet> bullets { get; set; } =
            GameConfigTools.GenConfigDict<string, bullet>();

        public static ImmutableDictionary<string, push_buff> push_buffs { get; set; } =
            GameConfigTools.GenConfigDict<string, push_buff>();

        public static ImmutableDictionary<string, lock_area> lock_areas { get; set; } =
            GameConfigTools.GenConfigDict<string, lock_area>();

        public static ImmutableDictionary<int, item> items { get; set; } = GameConfigTools.GenConfigDict<int, item>();

        public static ImmutableDictionary<string, weapon> weapons { get; set; } =
            GameConfigTools.GenConfigDict<string, weapon>();

        public static ImmutableDictionary<int, fish> fishs { get; set; } = GameConfigTools.GenConfigDict<int, fish>();

        public static IDictionary[] all_Immutable_dictionary =
        {
            skills, bad_wordss, caught_buffs, other_configs, bodys, show_texts, bullets, push_buffs, lock_areas, items,
            weapons, fishs
        };
    }

    [Serializable]
    public class Point : IGameConfig
    {
        public float x { get; set; }
        public float y { get; set; }
    }

    [Serializable]
    public class Buff : IGameConfig
    {
        public string buff_type { get; set; }
        public string buff_id { get; set; }
    }

    [Serializable]
    public class SimpleObj1 : IGameConfig
    {
        public int x { get; set; }
        public int y { get; set; }
        public float scale { get; set; }
    }

    [Serializable]
    public class SimpleObj3 : IGameConfig
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }

    [Serializable]
    public class SimpleObj2 : IGameConfig
    {
        public SimpleObj3 pos { get; set; }
        public SimpleObj3 rotate { get; set; }
        public float scale { get; set; }
    }

    [Serializable]
    public class SimpleObj4 : IGameConfig
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    [Serializable]
    public class SimpleObj5 : IGameConfig
    {
        public int width { get; set; }
        public int height { get; set; }
    }
}