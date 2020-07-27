using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using game_config;

namespace game_stuff
{
    public class Weapon
    {
        public readonly ImmutableDictionary<OpAction, ImmutableDictionary<int, Skill>> SkillGroups;

        public Weapon(ImmutableDictionary<OpAction, ImmutableDictionary<int, Skill>> skillGroups)
        {
            SkillGroups = skillGroups;
        }

        public void PickedBySomebody(CharacterStatus characterStatus)
        {
            foreach (var skill in SkillGroups.Select(keyValuePair => keyValuePair.Value)
                .SelectMany(immutableDictionary => immutableDictionary.Select(valuePair => valuePair.Value)))
            {
                skill.PickedBySomeOne(characterStatus);
            }
        }

        public static Weapon GenByConfig(weapon weapon)
        {
            var dictionary = weapon.Op1.ToDictionary(pair => pair.Key, pair => Skill.GenSkillById(pair.Value))
                .ToImmutableDictionary();
            var dictionary2 = weapon.Op2.ToDictionary(pair => pair.Key, pair => Skill.GenSkillById(pair.Value))
                .ToImmutableDictionary();
            var dictionary3 = weapon.Op3.ToDictionary(pair => pair.Key, pair => Skill.GenSkillById(pair.Value))
                .ToImmutableDictionary();
            var dictionary4 = weapon.Switch.ToDictionary(pair => pair.Key, pair => Skill.GenSkillById(pair.Value))
                .ToImmutableDictionary();

            var immutableDictionary =
                new Dictionary<OpAction, ImmutableDictionary<int, Skill>>()
                {
                    {OpAction.Op1, dictionary}, {OpAction.Op2, dictionary2}, {OpAction.Op3, dictionary3},
                    {OpAction.Switch, dictionary4}
                }.ToImmutableDictionary();

            var weapon1 = new Weapon(immutableDictionary);
            return weapon1;
        }
    }
}