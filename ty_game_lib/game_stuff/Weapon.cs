using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using game_config;

namespace game_stuff
{
    public class Weapon
    {
        public readonly ImmutableDictionary<SkillAction, ImmutableDictionary<int, Skill>> SkillGroups;

        public Weapon(ImmutableDictionary<SkillAction, ImmutableDictionary<int, Skill>> skillGroups)
        {
            SkillGroups = skillGroups;
        }

        public string LogUserString()
        {
            return (from variable in SkillGroups
                from keyValuePair in variable.Value
                select keyValuePair.Value.LogUser())
                .Aggregate("", (current, logUser) => current + logUser);
        }

        public void PickedBySomebody(CharacterStatus characterStatus)
        {
            foreach (var skill in SkillGroups.Select(keyValuePair => keyValuePair.Value)
                .SelectMany(immutableDictionary => immutableDictionary.Select(valuePair => valuePair.Value)))
            {
                skill.PickedBySomeOne(characterStatus);
            }

            if (characterStatus.Weapons.Count < TempConfig.WeaponNum)
            {
                characterStatus.Weapons.Add(characterStatus.Weapons.Count, this);
            }
            else
            {
                var characterStatusNowWeapon = (characterStatus.NowWeapon + 1) % TempConfig.WeaponNum;
                characterStatus.Weapons[characterStatusNowWeapon] = this;
                characterStatus.NowWeapon = characterStatusNowWeapon;
            }
        }

        public static Weapon GenByConfig(weapon weapon)
        {
            var dictionary = weapon.Op1.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                .ToImmutableDictionary();
            var dictionary2 = weapon.Op2.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                .ToImmutableDictionary();
            var dictionary3 = weapon.Op3.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                .ToImmutableDictionary();
            var dictionary4 = weapon.Switch.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                .ToImmutableDictionary();

            var immutableDictionary =
                new Dictionary<SkillAction, ImmutableDictionary<int, Skill>>
                {
                    {SkillAction.Op1, dictionary}, {SkillAction.Op2, dictionary2}, {SkillAction.Op3, dictionary3},
                    {SkillAction.Switch, dictionary4}
                }.ToImmutableDictionary();

            var weapon1 = new Weapon(immutableDictionary);
            return weapon1;
        }
    }
}