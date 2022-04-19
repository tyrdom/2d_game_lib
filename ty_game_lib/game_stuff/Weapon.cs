using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Weapon : ISaleStuff, ICanPickDrop, IPutInCage
    {
        public weapon_id WId { get; }

        public Dictionary<size, Dictionary<SkillAction, Dictionary<int, Skill>>>
            SkillGroups { get; }

        public ImmutableDictionary<size, Skill> BlockSkills { get; }
        public float BotRanges { get; }
        public float KeepDistance { get; }
        public ImmutableDictionary<SnipeAction, Snipe> Snipes { get; }

        public float[] ZoomStepMulti { get; }
        public Scope[] ZoomStepScopes { get; private set; }

        private Weapon(
            Dictionary<size, Dictionary<SkillAction, Dictionary<int, Skill>>> skillGroups,
            float botRanges,
            ImmutableDictionary<SnipeAction, Snipe> snipes,
            weapon_id wId,
            float[] zoomStepMulti,
            Scope[] zoomStepScopes, float keepDistance, ImmutableDictionary<size, Skill> blockSkills)
        {
            SkillGroups = skillGroups;
            BotRanges = botRanges;
            Snipes = snipes;
            WId = wId;
            ZoomStepScopes = zoomStepScopes;
            KeepDistance = keepDistance;
            BlockSkills = blockSkills;
            ZoomStepMulti = zoomStepMulti;
            InWhichMapInteractive = null;
        }

        public string LogUserString()
        {
            return (from variable in SkillGroups
                    from keyValuePair in variable.Value
                    from valuePair in keyValuePair.Value
                    select valuePair.Value.LogUser())
                .Aggregate("", (current, logUser) => current + "." + logUser);
        }

        private bool CanBePickUp(size bodySize)
        {
            return SkillGroups.TryGetValue(bodySize, out _);
        }

        public IMapInteractable? PickedBySomebody(CharacterStatus characterStatus)
        {
            foreach (var skill in
                SkillGroups.Values)
            {
                foreach (var variable in skill.Values)
                {
                    foreach (var value in variable.Values)
                    {
                        value.PickedBySomeOne(characterStatus);
                    }
                }
            }

            foreach (var keyValuePaiSkill in BlockSkills.Values)
            {
                keyValuePaiSkill.PickedBySomeOne(characterStatus);
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

        private static Dictionary<SkillAction, Dictionary<int, Skill>> GenASkillGroup(
            skill_group skillGroup)
        {
            
            var dictionary = skillGroup.Op1.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                ;
           
            var dictionary2 = skillGroup.Op2.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                ;
            var dictionary3 = skillGroup.Op3.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                ;
            var dictionary4 = skillGroup.Switch.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                ;

            var dictionary5 = skillGroup.ChargeOp1.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                ;
            var dictionary6 = skillGroup.ChargeOp2.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                ;
            var dictionary7 = skillGroup.ChargeOp3.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                ;
            var dictionary8 = skillGroup.ChargeSwitch.ToDictionary(pair => pair.Key,
                    pair => Skill.GenSkillById(pair.Value))
                ;
         var  aSkillGroup =
                new Dictionary<SkillAction, Dictionary<int, Skill>>
                {
                    {SkillAction.Op1, dictionary}, {SkillAction.Op2, dictionary2}, {SkillAction.Op3, dictionary3},
                    {SkillAction.Switch, dictionary4} 
                };

         if (dictionary5.Count>0)
         {
             aSkillGroup[SkillAction.ChargeOp1] = dictionary5;
         }
         if (dictionary6.Count>0)
         {
             aSkillGroup[SkillAction.ChargeOp2] = dictionary6;
         }
         if (dictionary7.Count>0)
         {
             aSkillGroup[SkillAction.ChargeOp3] = dictionary7;
         }
         if (dictionary8.Count>0)
         {
             aSkillGroup[SkillAction.ChargeSwitch] = dictionary8;
         }
            
            return aSkillGroup;
        }


        public static weapon_id GenId(string id)
        {
            if (id.TryStringToEnum(out weapon_id weaponId))
            {
                return weaponId;
            }

            throw new KeyNotFoundException($"cant find weapon id {id}");
        }

        public static Weapon GenById(string id)
        {
            if (id.TryStringToEnum(out weapon_id weaponId))
            {
                return GenById(weaponId);
            }

            throw new KeyNotFoundException($"cant find weapon id {id}");
        }

        public static Weapon GenById(weapon_id id)
        {
            if (CommonConfig.Configs.weapons.TryGetValue(id, out var weapon))
            {
                return GenByConfig(weapon);
            }

            throw new KeyNotFoundException($"cant find weapon id {id}");
        }

        public static Weapon GenByConfig(weapon weapon)
        {
            Dictionary<size, Dictionary<SkillAction, Dictionary<int, Skill>>> immutableDictionary =
                weapon.BodySizeUseAndSnipeSpeedFix.ToDictionary(x => x.body,
                    x =>
                    {
                        var argSkillGroup = x.skill_group;
                        if (CommonConfig.Configs.skill_groups.TryGetValue(argSkillGroup, out var skillGroup))
                        {
                            return GenASkillGroup(skillGroup);
                        }

                        throw new Exception($"not such a group name::{skillGroup} in weapon {weapon.id} ");
                    });
            var blockDic = weapon.BodySizeToBlockSkill
                .ToDictionary(x => x.body, x => Skill.GenSkillById(x.blockSkill)).ToImmutableDictionary();

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
                enumerable, scopes, weapon.KeepDistance, blockDic);
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

        public ImmutableArray<IActResult> ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            switch (interactive)
            {
                case MapInteract.RecycleCall:
                    characterStatus.RecycleWeapon(InWhichMapInteractive?.MapMarkId ?? -1);
                    return ImmutableArray<IActResult>.Empty;
                case MapInteract.PickCall:
                    var picAWeapon = characterStatus.PicAWeapon(this, InWhichMapInteractive?.MapMarkId ?? -1);
                    return picAWeapon == null
                        ? ImmutableArray<IActResult>.Empty
                        : new IActResult[] {new DropThings(new List<IMapInteractable> {picAWeapon})}.ToImmutableArray();

                default:
                    return ImmutableArray<IActResult>.Empty;
            }
        }

        public int GetId()
        {
            return (int) WId;
        }

        public int GetNum()
        {
            return 1;
        }
    }
}