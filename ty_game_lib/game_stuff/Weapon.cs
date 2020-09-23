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
        public int WId { get; }
        public ImmutableDictionary<SkillAction, ImmutableDictionary<int, Skill>> SkillGroups { get; }

        public ImmutableDictionary<BodySize, float> BodyCanUseToSpeedMulti { get; }
        public ImmutableArray<(float, SkillAction)> Ranges { get; }

        public ImmutableDictionary<SnipeAction, Snipe> Snipes { get; }

        public float[] ZoomStepMulti { get; }
        public Scope[] ZoomStepScopes { get; private set; }

        private Weapon(ImmutableDictionary<SkillAction, ImmutableDictionary<int, Skill>> skillGroups,
            ImmutableArray<(float, SkillAction)> ranges, ImmutableDictionary<SnipeAction, Snipe> snipes, int wId,
            float[] zoomStepMulti, Scope[] zoomStepScopes, ImmutableDictionary<BodySize, float> bodyCanUseToSpeedMulti)
        {
            SkillGroups = skillGroups;
            Ranges = ranges;
            Snipes = snipes;
            WId = wId;
            ZoomStepScopes = zoomStepScopes;
            BodyCanUseToSpeedMulti = bodyCanUseToSpeedMulti;
            ZoomStepMulti = zoomStepMulti;
        }

        public string LogUserString()
        {
            return (from variable in SkillGroups
                    from keyValuePair in variable.Value
                    select keyValuePair.Value.LogUser())
                .Aggregate("", (current, logUser) => current + logUser);
        }


        public bool CanBePickUp(BodySize bodySize)
        {
            return BodyCanUseToSpeedMulti.TryGetValue(bodySize, out _);
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

            ZoomStepScopes = ZoomStepMulti.Select(x => characterStatus.CharacterBody.Sight.StandardScope.GenNewScope(x))
                .ToArray();
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


            static SkillAction GetSkillActionByInt(int i)
            {
                return i switch
                {
                    0 => SkillAction.Op1,
                    1 => SkillAction.Op2,
                    2 => SkillAction.Op3,
                    _ => throw new Exception($"not good Act config {i}")
                };
            }

            var snipes = new Dictionary<SnipeAction, Snipe>();

            static Snipe? GetSnipeById(int id)
            {
                return TempConfig.Configs.snipes.TryGetValue(id, out var snipe)
                    ? new Snipe(snipe)
                    : null;
            }

            var snipeById1 = GetSnipeById(weapon.Snipe1);
            if (snipeById1 != null)
            {
                snipes[SnipeAction.SnipeOn1] = snipeById1;
            }

            var snipeById2 = GetSnipeById(weapon.Snipe2);
            if (snipeById2 != null)
            {
                snipes[SnipeAction.SnipeOn2] = snipeById2;
            }

            var snipeById3 = GetSnipeById(weapon.Snipe3);
            if (snipeById3 != null)
            {
                snipes[SnipeAction.SnipeOn3] = snipeById3;
            }


            var floats = weapon.BodySizeUseAndSpeed.ToDictionary(x =>
                {
                    return x.body switch
                    {
                        size.medium => BodySize.Medium,
                        size.small => BodySize.Small,
                        size.big => BodySize.Big,
                        size.@default => BodySize.Small,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                },
                x => x.speed_fix
            ).ToImmutableDictionary() ?? throw new Exception($"weapon can not be picked by any one {weapon.id}");

            var valueTuples = weapon.BotRange
                .Select(keyValuePair => (keyValuePair.Value, GetSkillActionByInt(keyValuePair.Key)))
                .ToList();

            valueTuples.Sort((x, y) => -x.Item1.CompareTo(x.Item1));
            var immutableList = valueTuples.ToImmutableArray();

            var weaponMaxRangeMultiAdd = weapon.MaxRangeMulti - 1;
            var enumerable = Enumerable.Range(1, weapon.ChangeRangeStep)
                .Select(x => 1 + x * weaponMaxRangeMultiAdd / weapon.ChangeRangeStep).ToArray();

            var scopes = enumerable.Select(x => Scope.StandardScope().GenNewScope(x)).ToArray();

            var weapon1 = new Weapon(immutableDictionary, immutableList, snipes.ToImmutableDictionary(), weapon.id,
                enumerable, scopes, floats);
            return weapon1;
        }
    }
}