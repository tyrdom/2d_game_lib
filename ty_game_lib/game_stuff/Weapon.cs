using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Weapon : ISaleStuff, ICanPickDrop, ICanPutInMapInteractable, IPutInCage
    {
        public int WId { get; }

        public ImmutableDictionary<size, ImmutableDictionary<SkillAction, ImmutableDictionary<int, Skill>>>
            SkillGroups { get; }


        public float BotRanges { get; }

        public ImmutableDictionary<SnipeAction, Snipe> Snipes { get; }

        public float[] ZoomStepMulti { get; }
        public Scope[] ZoomStepScopes { get; private set; }

        private Weapon(
            ImmutableDictionary<size, ImmutableDictionary<SkillAction,
                ImmutableDictionary<int, Skill>>> skillGroups,
            float botRanges,
            ImmutableDictionary<SnipeAction, Snipe> snipes,
            int wId,
            float[] zoomStepMulti,
            Scope[] zoomStepScopes)
        {
            SkillGroups = skillGroups;
            BotRanges = botRanges;
            Snipes = snipes;
            WId = wId;
            ZoomStepScopes = zoomStepScopes;
            ZoomStepMulti = zoomStepMulti;
            InWhichMapInteractive = null;
        }

        public string LogUserString()
        {
            return (from variable in SkillGroups
                    from keyValuePair in variable.Value
                    from valuePair in keyValuePair.Value
                    select valuePair.Value.LogUser())
                .Aggregate("", (current, logUser) => current +"."+ logUser);
        }

        private bool CanBePickUp(size bodySize)
        {
            return SkillGroups.TryGetValue(bodySize, out _);
        }

        public IMapInteractable? PickedBySomebody(CharacterStatus characterStatus)
        {
            foreach (var skill in
                SkillGroups.SelectMany(keyValuePair => keyValuePair.Value)
                    .SelectMany(x => x.Value)
                    .Select(immutableDictionary => immutableDictionary.Value))
            {
                skill.PickedBySomeOne(characterStatus);
            }

            ZoomStepScopes = ZoomStepMulti.Select(x => characterStatus.GetStandardScope().GenNewScope(x))
                .ToArray();
            characterStatus.ResetSnipe();
            var weapons = characterStatus.GetWeapons();
            if (weapons.Count < characterStatus.GetNowMaxWeaponSlotNum())
            {
                weapons.Add(weapons.Count, this);
                return null;
            }

            var characterStatusNowWeapon = characterStatus.NowWeapon;
            var characterStatusWeapon = weapons[characterStatusNowWeapon];
            var dropAsIMapInteractable = characterStatusWeapon.DropAsIMapInteractable(characterStatus.GetPos());
            weapons[characterStatusNowWeapon] = this;
            return dropAsIMapInteractable;
        }

        private static ImmutableDictionary<SkillAction, ImmutableDictionary<int, Skill>> GenASkillGroup(
            skill_group skillGroup)
        {
            var dictionary = skillGroup.Op1.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                .ToImmutableDictionary();
            var dictionary2 = skillGroup.Op2.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                .ToImmutableDictionary();
            var dictionary3 = skillGroup.Op3.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                .ToImmutableDictionary();
            var dictionary4 = skillGroup.Switch.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                .ToImmutableDictionary();

            var immutableDictionary =
                new Dictionary<SkillAction, ImmutableDictionary<int, Skill>>
                {
                    {SkillAction.Op1, dictionary}, {SkillAction.Op2, dictionary2}, {SkillAction.Op3, dictionary3},
                    {SkillAction.Switch, dictionary4}
                }.ToImmutableDictionary();
            return immutableDictionary;
        }


        public static Weapon GenById(int id)
        {
            if (CommonConfig.Configs.weapons.TryGetValue(id, out var weapon))
            {
                return GenByConfig(weapon);
            }

            throw new KeyNotFoundException($"cant find weapon id {id}");
        }

        public static Weapon GenByConfig(weapon weapon)
        {
            var immutableDictionary = weapon.BodySizeUseAndSnipeSpeedFix.ToDictionary(x => x.body,
                x =>
                {
                    var argSkillGroup = x.skill_group;
                    if (CommonConfig.Configs.skill_groups.TryGetValue(argSkillGroup, out var skillGroup))
                    {
                        return GenASkillGroup(skillGroup);
                    }

                    throw new Exception($"not such a group name::{skillGroup} ");
                }).ToImmutableDictionary();


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

            var weaponChangeRangeStep = weapon.ChangeRangeStep;

            static Snipe? GetSnipeById(int id, ImmutableDictionary<size, float> ff, int aWeaponChangeRangeStep)
            {
                return CommonConfig.Configs.snipes.TryGetValue(id, out var snipe)
                    ? new Snipe(snipe, ff, aWeaponChangeRangeStep)
                    : null;
            }

            var floats = weapon.BodySizeUseAndSnipeSpeedFix.ToDictionary(x => x.body,
                x => x.snipe_speed_fix
            ).ToImmutableDictionary() ?? throw new Exception($"weapon can not be picked by any one {weapon.id}");
            var snipeById1 = GetSnipeById(weapon.Snipe1, floats, weaponChangeRangeStep);
            if (snipeById1 != null)
            {
                snipes[SnipeAction.SnipeOn1] = snipeById1;
            }

            var snipeById2 = GetSnipeById(weapon.Snipe2, floats, weaponChangeRangeStep);
            if (snipeById2 != null)
            {
                snipes[SnipeAction.SnipeOn2] = snipeById2;
            }

            var snipeById3 = GetSnipeById(weapon.Snipe3, floats, weaponChangeRangeStep);
            if (snipeById3 != null)
            {
                snipes[SnipeAction.SnipeOn3] = snipeById3;
            }


            var weaponMaxRangeMultiAdd = weapon.MaxRangeMulti - 1;
            var enumerable = Enumerable.Range(1, weaponChangeRangeStep)
                .Select(x => 1 + x * weaponMaxRangeMultiAdd / weaponChangeRangeStep).ToArray();

            var scopes = enumerable.Select(x => Scope.StandardScope().GenNewScope(x)).ToArray();

            var weapon1 = new Weapon(immutableDictionary, weapon.BotRange, snipes.ToImmutableDictionary(), weapon.id,
                enumerable, scopes);
            return weapon1;
        }

        public IMapInteractable PutInteractable(TwoDPoint pos, bool isActive)
        {
            return CanPutInMapInteractableStandard.PutInMapInteractable(pos, this);
        }

        public IMapInteractable? InWhichMapInteractive { get; set; }

        public IMapInteractable DropAsIMapInteractable(TwoDPoint pos)
        {
            return CanPutInMapInteractableStandard.PutInMapInteractable(pos, this);
        }

        public bool CanInterActOneBy(CharacterStatus characterStatus)
        {
            return CanBePickUp(characterStatus.CharacterBody.GetSize());
        }

        public bool CanInterActTwoBy(CharacterStatus characterStatus)
        {
            return true;
        }

        public IActResult? ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            switch (interactive)
            {
                case MapInteract.RecycleCall:
                    characterStatus.RecycleWeapon();
                    return null;
                case MapInteract.PickCall:
                    var picAWeapon = characterStatus.PicAWeapon(this);
                    return picAWeapon == null
                        ? (IActResult?) null
                        : new DropThings(new List<IMapInteractable> {picAWeapon});

                default:
                    throw new ArgumentOutOfRangeException(nameof(interactive), interactive, null);
            }
        }

        public int GetId()
        {
            return WId;
        }

        public int GetNum()
        {
            return 1;
        }
    }
}