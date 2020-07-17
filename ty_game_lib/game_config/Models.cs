using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace game_config
{
    public class Bullet : IGameConfig
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

    public class Caught_buff : IGameConfig
    {
        public string id { get; set; }
        public int LastTick { get; set; }
        public Point[] CatchPoints { get; set; }
        public string TrickSkill { get; set; }
    }

    public class Item : IGameConfig
    {
        public int id { get; set; }
        public bool IsMoney { get; set; }
        public int ShowType { get; set; }
        public string Name { get; set; }
        public string another_name { get; set; }
        public string Icon { get; set; }
    }

    public class Lock_area : IGameConfig
    {
        public string id { get; set; }
        public int ShapeType { get; set; }
        public float[] ShapeParams { get; set; }
        public Point LocalPos { get; set; }
        public int LocalRotate { get; set; }
    }

    public class Push_buff : IGameConfig
    {
        public string id { get; set; }
        public int LastTick { get; set; }
        public int BuffType { get; set; }
        public float PushForce { get; set; }
        public int PushType { get; set; }
        public Point[] FixVector { get; set; }
        public float UpForce { get; set; }
    }

    public class Show_text : IGameConfig
    {
        public string id { get; set; }
        public string chinese { get; set; }
    }

    public class Skill : IGameConfig
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

    public class Weapon : IGameConfig
    {
        public string id { get; set; }
        public Dictionary<int, string> Op1 { get; set; }
        public Dictionary<int, string> Op2 { get; set; }
        public Dictionary<int, string> Op3 { get; set; }
    }

    public static class ResNames
    {
        public static Dictionary<Type, string> NamesDictionary = new Dictionary<Type, string>
        {
            {typeof(Bullet), "bullet_s.json"}, {typeof(Caught_buff), "caught_buff_s.json"},
            {typeof(Item), "item_s.json"}, {typeof(Lock_area), "lock_area_s.json"},
            {typeof(Push_buff), "push_buff_s.json"}, {typeof(Show_text), "show_text_s.json"},
            {typeof(Skill), "skill_s.json"}, {typeof(Weapon), "weapon_s.json"}
        };
    }

    public static class Content
    {
        public static ImmutableDictionary<string, Bullet> Bullets = GameConfigTools.GenConfigDict<string, Bullet>();

        public static ImmutableDictionary<string, Caught_buff> Caught_buffs =
            GameConfigTools.GenConfigDict<string, Caught_buff>();

        public static ImmutableDictionary<int, Item> Items = GameConfigTools.GenConfigDict<int, Item>();

        public static ImmutableDictionary<string, Lock_area> Lock_areas =
            GameConfigTools.GenConfigDict<string, Lock_area>();

        public static ImmutableDictionary<string, Push_buff> Push_buffs =
            GameConfigTools.GenConfigDict<string, Push_buff>();

        public static ImmutableDictionary<string, Show_text> Show_texts =
            GameConfigTools.GenConfigDict<string, Show_text>();

        public static ImmutableDictionary<string, Skill> Skills = GameConfigTools.GenConfigDict<string, Skill>();
        public static ImmutableDictionary<string, Weapon> Weapons = GameConfigTools.GenConfigDict<string, Weapon>();
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