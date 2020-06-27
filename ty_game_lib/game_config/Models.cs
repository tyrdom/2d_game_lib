using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace game_config
{
    public class Bullet : IGameConfig
    {
        public int id { get; set; }
        public int CdMs { get; set; }
        public float HarmMulti { get; set; }
        public int SkillKey { get; set; }
        public string Info { get; set; }
    }

    public class Character : IGameConfig
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public int[] items { get; set; }
        public string Vocation { get; set; }
        public int[] ActiveSkills { get; set; }
        public int[] PassiveSkills { get; set; }
        public int BaseAttribute { get; set; }
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
        public int CdMs { get; set; }
        public float HarmMulti { get; set; }
        public int SkillKey { get; set; }
        public string Info { get; set; }
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
            {typeof(Bullet), "bullet_s.json"}, {typeof(Character), "character_s.json"}, {typeof(Item), "item_s.json"},
            {typeof(Show_text), "show_text_s.json"}, {typeof(Skill), "skill_s.json"}, {typeof(Weapon), "weapon_s.json"}
        };
    }

    public static class Content
    {
        public static ImmutableDictionary<int, Bullet> Bullets = GameConfigTools.GenConfigDict<Bullet>();
        public static ImmutableDictionary<int, Character> Characters = GameConfigTools.GenConfigDict<Character>();
        public static ImmutableDictionary<int, Item> Items = GameConfigTools.GenConfigDict<Item>();
        public static ImmutableDictionary<int, Show_text> Show_texts = GameConfigTools.GenConfigDict<Show_text>();
        public static ImmutableDictionary<int, Skill> Skills = GameConfigTools.GenConfigDict<Skill>();
        public static ImmutableDictionary<int, Weapon> Weapons = GameConfigTools.GenConfigDict<Weapon>();
    }
}