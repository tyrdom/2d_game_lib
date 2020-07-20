using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace game_config
{
    public class body : IGameConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public size id { get; set; }

        public float mass { get; set; }
        public float rad { get; set; }
    }

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

    public class caught_buff : IGameConfig
    {
        public string id { get; set; }
        public int LastTick { get; set; }
        public Point[] CatchPoints { get; set; }
        public string TrickSkill { get; set; }
    }

    public class item : IGameConfig
    {
        public int id { get; set; }
        public bool IsMoney { get; set; }
        public int ShowType { get; set; }
        public string Name { get; set; }
        public string another_name { get; set; }
        public string Icon { get; set; }
    }

    public class lock_area : IGameConfig
    {
        public string id { get; set; }
        public int ShapeType { get; set; }
        public float[] ShapeParams { get; set; }
        public Point LocalPos { get; set; }
        public int LocalRotate { get; set; }
    }

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

    public enum push_buff_PushType
    {
        @Center,
        @Vector
    }

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

    public class show_text : IGameConfig
    {
        public string id { get; set; }
        public string chinese { get; set; }
    }

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

    public class weapon : IGameConfig
    {
        public string id { get; set; }
        public Dictionary<int, string> Op1 { get; set; }
        public Dictionary<int, string> Op2 { get; set; }
        public Dictionary<int, string> Op3 { get; set; }
    }

    public enum size
    {
        @small,
        @medium,
        @big
    }

    public static class ResNames
    {
        public static Dictionary<Type, string> NamesDictionary = new Dictionary<Type, string>
        {
            {typeof(body), "body_s.json"}, {typeof(bullet), "bullet_s.json"},
            {typeof(caught_buff), "caught_buff_s.json"}, {typeof(item), "item_s.json"},
            {typeof(lock_area), "lock_area_s.json"}, {typeof(other_config), "other_config_s.json"},
            {typeof(push_buff), "push_buff_s.json"}, {typeof(show_text), "show_text_s.json"},
            {typeof(skill), "skill_s.json"}, {typeof(weapon), "weapon_s.json"}
        };
    }

    public static class Content
    {
        public static readonly ImmutableDictionary<size, body> bodys = GameConfigTools.GenConfigDict<size, body>();

        public static readonly ImmutableDictionary<string, bullet> bullets =
            GameConfigTools.GenConfigDict<string, bullet>();

        public static readonly ImmutableDictionary<string, caught_buff> caught_buffs =
            GameConfigTools.GenConfigDict<string, caught_buff>();

        public static readonly ImmutableDictionary<int, item> items = GameConfigTools.GenConfigDict<int, item>();

        public static readonly ImmutableDictionary<string, lock_area> lock_areas =
            GameConfigTools.GenConfigDict<string, lock_area>();

        public static readonly ImmutableDictionary<int, other_config> other_configs =
            GameConfigTools.GenConfigDict<int, other_config>();

        public static readonly ImmutableDictionary<string, push_buff> push_buffs =
            GameConfigTools.GenConfigDict<string, push_buff>();

        public static readonly ImmutableDictionary<string, show_text> show_texts =
            GameConfigTools.GenConfigDict<string, show_text>();

        public static readonly ImmutableDictionary<string, skill> skills =
            GameConfigTools.GenConfigDict<string, skill>();

        public static readonly ImmutableDictionary<string, weapon> weapons =
            GameConfigTools.GenConfigDict<string, weapon>();
    }

    public class Point : IGameConfig
    {
        public float x { get; set; }
        public float y { get; set; }
    }

    public class Buff : IGameConfig
    {
        public string buff_type { get; set; }
        public string buff_id { get; set; }
    }
}