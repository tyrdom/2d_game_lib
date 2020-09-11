using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using game_config;

namespace game_stuff
{
    public class Weapon
    {
        public ImmutableDictionary<SkillAction, ImmutableDictionary<int, Skill>> SkillGroups { get; }

        public ImmutableArray<(float, SkillAction)> Ranges { get; }

        public Weapon(ImmutableDictionary<SkillAction, ImmutableDictionary<int, Skill>> skillGroups,
            ImmutableArray<(float, SkillAction)> ranges)
        {
            SkillGroups = skillGroups;
            Ranges = ranges;
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


            static SkillAction GetByInt(int i)
            {
                if (i == 0)
                {
                    return SkillAction.Op1;
                }

                if (i == 1)
                {
                    return SkillAction.Op2;
                }

                if (i == 2)
                {
                    return SkillAction.Op3;
                }

                throw new Exception($"not good Act config {i}");
            }

            var valueTuples = new List<(float, SkillAction)>();
            foreach (var keyValuePair in weapon.BotRange)
            {
                valueTuples.Add((keyValuePair.Value, GetByInt(keyValuePair.Key)));
            }

            valueTuples.Sort((x, y) => -x.Item1.CompareTo(x.Item1));
            var immutableList = valueTuples.ToImmutableArray();
            var weapon1 = new Weapon(immutableDictionary, immutableList);
            return weapon1;
        }
    }
}