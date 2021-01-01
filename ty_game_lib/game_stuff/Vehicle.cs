using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Vehicle : ISaleStuff, IMoveBattleAttrModel, ICanDrop, ICanPutInMapInteractable
    {
        public float TrapAtkMulti { get; set; }

        public static Vehicle GenById(int id)
        {
            if (LocalConfig.Configs.vehicles.TryGetValue(id, out var vehicle))
            {
                return GenByConfig(vehicle);
            }

            throw new DirectoryNotFoundException();
        }

        private static Vehicle GenByConfig(vehicle vehicle)
        {
            var bodySize = LocalConfig.GetBodySize(vehicle.BodyId);
            var tickByTime = CommonConfig.GetTickByTime(vehicle.DestoryDelayTime);

            var genByBulletId = Bullet.GenById(vehicle.DestoryBullet);
            var genSkillById = Skill.GenSkillById(vehicle.OutActSkill);

            var vehicleVScope = vehicle.VScope;
            var vScope = vehicleVScope == null
                ? Scope.StandardScope()
                : new Scope(new TwoDVector(vehicleVScope.x, vehicleVScope.y));


            var dictionary = new Dictionary<int, Weapon>();

            for (var index = 0; index < vehicle.Weapons.Length; index++)
            {
                var vehicleWeapon = vehicle.Weapons[index];

                var genById = Weapon.GenById(vehicleWeapon);
                if (index < vehicle.MaxWeaponSlot)
                {
                    dictionary[index] = genById;
                }
                else
                {
                    break;
                }
            }

            return new Vehicle(bodySize, vScope, dictionary, genByBulletId, tickByTime,
                vehicle.MaxWeaponSlot, genSkillById, vehicle.AttrId, vehicle.id);
        }

        private Vehicle(BodySize size, Scope scope, Dictionary<int, Weapon> weapons, Bullet destroyBullet,
            uint destroyTick, int weaponCarryMax,
            Skill outAct, int baseAttrId, int vId)
        {
            var genBaseAttrById = GameTools.GenBaseAttrById(baseAttrId);
            SurvivalStatus = SurvivalStatus.GenByConfig(genBaseAttrById);
            AttackStatus = AttackStatus.GenByConfig(genBaseAttrById);
            RegenEffectStatus = RegenEffectStatus.GenBaseByAttr(genBaseAttrById);
            AbsorbStatus = AbsorbStatus.GenBaseByAttr(genBaseAttrById);
            MaxTrapNum = genBaseAttrById.MaxTrapNum;
            TrapAtkMulti = genBaseAttrById.TrapAtkMulti;
            TrapSurvivalMulti = genBaseAttrById.TrapSurvivalMulti;
            Size = size;
            MaxMoveSpeed = genBaseAttrById.MoveMaxSpeed;
            MinMoveSpeed = genBaseAttrById.MoveMinSpeed;
            AddMoveSpeed = genBaseAttrById.MoveAddSpeed;
            Scope = scope;
            Weapons = weapons;
            DestroyBullet = destroyBullet;
            DestroyTick = destroyTick;
            WeaponCarryMax = weaponCarryMax;
            NowAmmo = 0;
            MaxAmmo = genBaseAttrById.MaxAmmo;
            OutAct = outAct;
            BaseAttrId = baseAttrId;
            VId = vId;
            WhoDriveOrCanDrive = null;
            NowDsTick = 0;
            IsDsOn = false;
            RecycleMulti = genBaseAttrById.RecycleMulti;
        }

        public float TrapSurvivalMulti { get; set; }

        public uint MaxTrapNum { get; set; }

        public int VId { get; }
        public CharacterStatus? WhoDriveOrCanDrive { get; set; }
        public BodySize Size { get; }

        public float MaxMoveSpeed { get; set; }
        public float MinMoveSpeed { get; }
        public float AddMoveSpeed { get; }
        public int BaseAttrId { get; }
        public Scope Scope { get; }
        public Dictionary<int, Weapon> Weapons { get; }

        public int NowAmmo { get; private set; }

        private int MaxAmmo { get; set; }
        public int WeaponCarryMax { get; }
        private uint DestroyTick { get; }

        public bool IsDsOn { get; private set; }
        private uint NowDsTick { get; set; }
        private Bullet DestroyBullet { get; }
        public SurvivalStatus SurvivalStatus { get; }
        public AbsorbStatus AbsorbStatus { get; }
        public RegenEffectStatus RegenEffectStatus { get; }

        public float RecycleMulti { get; private set; }

        public (bool isBroken, Bullet? destroyBullet) GoATickCheckSurvival()
        {
            if (IsDsOn)
            {
                NowDsTick++;
                if (NowDsTick > DestroyTick)
                {
                    return (IsDsOn, DestroyBullet);
                }
            }

            var goATickAndCheckAlive = SurvivalStatus.GoATickAndCheckAlive();
            if (!goATickAndCheckAlive)
            {
                IsDsOn = true;
            }

            return (IsDsOn, null);
        }

        public void SurvivalStatusRefresh(Vector<float> survivalAboutPassiveEffects)
        {
            BattleUnitMoverStandard.SurvivalStatusRefresh(survivalAboutPassiveEffects, this);
        }

        public AttackStatus AttackStatus { get; }

        public void AttackStatusRefresh(Vector<float> atkAboutPassiveEffects)
        {
            BattleUnitMoverStandard.AtkStatusRefresh(atkAboutPassiveEffects, this);
        }

        public void OtherStatusRefresh(Vector<float> otherAttrPassiveEffects)
        {
            BattleUnitMoverStandard.OtherStatusRefresh(otherAttrPassiveEffects, this);
        }

        public Skill OutAct { get; }

        public void AddAmmo(int ammoAdd) => NowAmmo = Math.Min(MaxAmmo, NowAmmo + ammoAdd);

        public void SetAmmo(int ammo) => NowAmmo = ammo;
        public void ReloadAmmo(float reloadMulti)
        {
            NowAmmo = (int) Math.Min(MaxAmmo, NowAmmo + MaxAmmo * reloadMulti * RegenEffectStatus.ReloadEffect);
        }

        public void PassiveEffectChangeOther(Vector<float> otherAttrPassiveEffects,
            (int MaxAmmo, float MoveMaxSpeed, float MoveMinSpeed, float MoveAddSpeed, int StandardPropMaxStack, float
                RecycleMulti) otherBaseStatus)
        {
            var lossAmmo = MaxAmmo - NowAmmo;
            var (maxAmmo, moveMaxSpeed, _, moveAddSpeed, _, recycleMulti) = otherBaseStatus;
            MaxAmmo = (int) (maxAmmo * (1f + otherAttrPassiveEffects[0]));
            NowAmmo = Math.Max(0, MaxAmmo - lossAmmo);
            var max = otherAttrPassiveEffects[1];
            MaxMoveSpeed = moveMaxSpeed * (1f + max / (max + 1f));
            var add = otherAttrPassiveEffects[2];
            MaxMoveSpeed = moveAddSpeed * (1f + add / (add + 1f));
            RecycleMulti = recycleMulti * (1f + otherAttrPassiveEffects[4]);
        }

        public void PassiveEffectChangeTrap(Vector<float> trapAdd,
            (float TrapAtkMulti, float TrapSurvivalMulti) trapBaseAttr)
        {
            BattleUnitMoverStandard.PassiveEffectChangeTrap(trapAdd, trapBaseAttr, this);
        }


        public IMapInteractable? InWhichMapInteractive { get; set; }

        public IMapInteractable DropAsIMapInteractable(TwoDPoint pos)
        {
            switch (InWhichMapInteractive)
            {
                case null:
                    return new VehicleCanIn(this, pos);

                case VehicleCanIn vehicleCanIn:
                    vehicleCanIn.ReLocate(pos);
                    return vehicleCanIn;
                default:
                    throw new ArgumentOutOfRangeException(nameof(InWhichMapInteractive));
            }
        }

        public bool CanInterActOneBy(CharacterStatus characterStatus)
        {
            var b = WhoDriveOrCanDrive == null || WhoDriveOrCanDrive == characterStatus;
            return b && characterStatus.NowVehicle == null;
        }

        public bool CanInterActTwoBy(CharacterStatus characterStatus)
        {
            return WhoDriveOrCanDrive == null || WhoDriveOrCanDrive == characterStatus;
        }

        public IActResult? ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            return interactive switch
            {
                MapInteract.InVehicleCall => new DropThings(characterStatus.GetInAVehicle(this)),
                MapInteract.KickVehicleCall => new DropThings(KickBySomeBody(characterStatus.GetPos())),
                _ => throw new ArgumentOutOfRangeException(nameof(interactive), interactive, null)
            };
        }

        public int GetId()
        {
            return VId;
        }

        public int GetNum()
        {
            return 1;
        }

        private IEnumerable<IMapInteractable> KickBySomeBody(TwoDPoint pos)
        {
            var opos = InWhichMapInteractive == null ? pos : ((IAaBbBox) InWhichMapInteractive).GetAnchor().GetMid(pos);

            var mapIntractable = Weapons.Select(x => x.Value.DropAsIMapInteractable(opos));
            Weapons.Clear();
            return mapIntractable;
        }

        public void FullAmmo()
        {
            NowAmmo = MaxAmmo;
        }

        public void Sign(CharacterStatus characterStatus)
        {
            WhoDriveOrCanDrive = characterStatus;
            var characterStatusPassiveTraits = characterStatus.PassiveTraits;
            RefreshByPass(characterStatusPassiveTraits);

            DestroyBullet.Sign(characterStatus);
        }


        private void RefreshByPass(Dictionary<int, PassiveTrait> characterStatusPassiveTraits)
        {
            var passiveTraits =
                characterStatusPassiveTraits.Values.Where(x => x.PassiveTraitEffect
                    is IPassiveTraitEffectForVehicle);
            var groupBy = passiveTraits.GroupBy(x => x.GetType());
            foreach (var grouping in groupBy)
            {
                var firstOrDefault = grouping.FirstOrDefault();
                if (firstOrDefault == null)
                {
                    continue;
                }

                if (firstOrDefault.PassiveTraitEffect is IPassiveTraitEffectForVehicle passiveTraitEffect)
                {
                    var aggregate = grouping.Aggregate(Vector<float>.Zero,
                        (s, x) => s + x.PassiveTraitEffect.GenEffect(x.Level).GetVector());
                    switch (passiveTraitEffect)
                    {
                        case AbsorbAboutPassiveEffect _:
                            AbsorbStatusRefresh(aggregate);
                            break;
                        case AtkAboutPassiveEffect _:
                            AttackStatusRefresh(aggregate);
                            break;
                        case OtherAttrPassiveEffect _:
                            OtherStatusRefresh(aggregate);
                            break;
                        case RegenPassiveEffect _:
                            RegenStatusRefresh(aggregate);
                            break;
                        case SurvivalAboutPassiveEffect _:
                            SurvivalStatusRefresh(aggregate);
                            break;
                        case TrapEffect _:
                            TrapAboutRefresh(aggregate);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(passiveTraitEffect));
                    }
                }
            }
        }

        public void TrapAboutRefresh(Vector<float> trapAdd)
        {
            BattleUnitMoverStandard.TrapAboutRefresh(trapAdd, this);
        }

        public void RegenStatusRefresh(Vector<float> rPassiveEffects)
        {
            BattleUnitMoverStandard.RegenStatusRefresh(rPassiveEffects, this);
        }


        public void AbsorbDamage(uint genDamageShardedDamage, uint genDamageShardedNum, uint shardedDamage)
        {
            SurvivalStatus.AbsorbDamage(genDamageShardedDamage, genDamageShardedNum, AbsorbStatus, shardedDamage);
        }

        public void AbsorbStatusRefresh(Vector<float> vector)
        {
            BattleUnitMoverStandard.AbsorbStatusRefresh(vector, this);
        }
    }
}