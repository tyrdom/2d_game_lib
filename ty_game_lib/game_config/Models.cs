using System;
using System.Collections.Generic;


namespace game_config
{
    public class Bullet : IGameConfig
    {
        public int id { get; set; }
        public int ShapeType { get; set; }
        public int[] ShapeParams { get; set; }
        public SimpleObj1 LocalPos { get; set; }
        public int LocalRotate { get; set; }
        public int SuccessAntiActBuffConfigToOpponent { get; set; }
        public int SuccessTrickSkillToSelf { get; set; }
        public Dictionary<int, int> FailActBuffConfigToSelf { get; set; }
        public int PauseToCaster { get; set; }
        public int PauseToOpponent { get; set; }
        public int TargetType { get; set; }
        public int Tough { get; set; }
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

    public class Show_text : IGameConfig
    {
        public int id { get; set; }
        public string chinese { get; set; }
    }

    public class Skill : IGameConfig
    {
        public int id { get; set; }
        public int BaseTough { get; set; }
        public Dictionary<uint, int> LaunchTickToBullet { get; set; }
        public SimpleObj2[] Moves { get; set; }
        public int MoveStartTick { get; set; }
        public int HomingStartTick { get; set; }
        public int HomingEndTick { get; set; }
        public int SkillMustTick { get; set; }
        public int ComboInputStartTick { get; set; }
        public int SkillMaxTick { get; set; }
        public int NextCombo { get; set; }
    }

    public class Weapon : IGameConfig
    {
        public int id { get; set; }
        public Dictionary<int, int> Op1 { get; set; }
        public Dictionary<int, int> Op2 { get; set; }
        public Dictionary<int, int> Op3 { get; set; }
    }

    public static class ResNames
    {
        public static Dictionary<Type, string> NamesDictionary = new Dictionary<Type, string>
        {
            {typeof(Bullet), "bullet_s.json"}, {typeof(Item), "item_s.json"}, {typeof(Show_text), "show_text_s.json"},
            {typeof(Skill), "skill_s.json"}, {typeof(Weapon), "weapon_s.json"}
        };
    }

    public static class Content
    {
        public static Dictionary<int, Bullet> Bullets = GameConfigTools.GenConfigDict<Bullet>();
        public static Dictionary<int, Item> Items = GameConfigTools.GenConfigDict<Item>();
        public static Dictionary<int, Show_text> Show_texts = GameConfigTools.GenConfigDict<Show_text>();
        public static Dictionary<int, Skill> Skills = GameConfigTools.GenConfigDict<Skill>();
        public static Dictionary<int, Weapon> Weapons = GameConfigTools.GenConfigDict<Weapon>();
    }

    public class SimpleObj1 : IGameConfig
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class SimpleObj2 : IGameConfig
    {
        public float x { get; set; }
        public float y { get; set; }
    }
}