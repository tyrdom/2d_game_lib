using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Vehicle : ISaleStuff, IMoveBattleAttrModel, ICanPickDrop, ICanPutInMapInteractable
    {
        public float TrapAtkMulti { get; set; }

        public static Vehicle GenById(string id)
        {
            if (id.TryStringToEnum(out vehicle_id eId))
            {
                return GenById(eId);
            }

            throw new KeyNotFoundException();
        }

        public static Vehicle GenById(vehicle_id id)
        {
            if (CommonConfig.Configs.vehicles.TryGetValue(id, out var vehicle))
            {
                return GenByConfig(vehicle);
            }

            throw new KeyNotFoundException();
        }

        private static Vehicle GenByConfig(vehicle vehicle)
        {
            var bodySize = vehicle.BodyId;
            var tickByTime = vehicle.DestoryDelayTime;

            var genByBulletId = Bullet.GenById(vehicle.DestoryBullet);
            var genSkillById = Skill.GenSkillById(vehicle.OutActSkill);

            var vehicleVScope = vehicle.VScope;
            var vScope = new Scope(new TwoDVector(vehicleVScope.x, vehicleVScope.y));


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

        private Vehicle(size size, Scope standardScope, Dictionary<int, Weapon> weapons, Bullet destroyBullet,
            uint destroyTick, int weaponCarryMax,
            Skill outAct, int baseAttrId, vehicle_id vId)
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
            ListenRange = genBaseAttrById.ListenRange;
            StandardScope = standardScope;
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
            IsDestroyOn = false;
            RecycleMulti = genBaseAttrById.RecycleMulti;
        }

        public float TrapSurvivalMulti { get; set; }

        public uint MaxTrapNum { get; set; }

        public vehicle_id VId { get; }
        public CharacterStatus? WhoDriveOrCanDrive { get; set; }
        public size Size { get; }

        public float MaxMoveSpeed { get; private set; }
        public float MinMoveSpeed { get; }
        public float AddMoveSpeed { get; }
        public float ListenRange { get; }
        public int BaseAttrId { get; }
        public Scope StandardScope { get; }
        public Dictionary<int, Weapon> Weapons { get; }

        public int NowAmmo { get; set; }

        public int MaxAmmo { get; set; }
        public int WeaponCarryMax { get; }
        private uint DestroyTick { get; }

        public bool IsDestroyOn { get; private set; }
        private uint NowDsTick { get; set; }
        private Bullet DestroyBullet { get; }
        public SurvivalStatus SurvivalStatus { get; }
        public AbsorbStatus AbsorbStatus { get; }
        public RegenEffectStatus RegenEffectStatus { get; }

        public float RecycleMulti { get; private set; }

        public (bool isBroken, Bullet? destroyBullet, Weapon[] weapons) GoATickCheckSurvival()
        {
            if (IsDestroyOn)
            {
                NowDsTick++;
                if (NowDsTick > DestroyTick)
                {
                    return (IsDestroyOn, DestroyBullet, Weapons.Values.ToArray());
                }
            }

            var goATickAndCheckAlive = SurvivalStatus.GoATickAndCheckAlive();
            if (!goATickAndCheckAlive)
            {
                IsDestroyOn = true;
            }

            return (IsDestroyOn, null, new Weapon[] { });
        }

        public void SurvivalStatusRefresh(float[] survivalAboutPassiveEffects)
        {
            BattleUnitMoverStandard.SurvivalStatusRefresh(survivalAboutPassiveEffects, this);
        }

        public AttackStatus AttackStatus { get; }


        public void AttackStatusRefresh(float[] atkAboutPassiveEffects)
        {
            BattleUnitMoverStandard.AtkStatusRefresh(atkAboutPassiveEffects, this);
        }

        public void OtherStatusRefresh(float[] otherAttrPassiveEffects)
        {
            BattleUnitMoverStandard.OtherStatusRefresh(otherAttrPassiveEffects, this);
        }

        public Skill OutAct { get; }

        public void AddAmmo(int ammoAdd) => NowAmmo = Math.Min(MaxAmmo, NowAmmo + ammoAdd);

        public void SetAmmo(int ammo) => NowAmmo = ammo;

        public void ReloadAmmo(float reloadMulti)
        {
            BattleUnitMoverStandard.ReloadAmmo(this, reloadMulti);
        }

        public void PassiveEffectChangeOther(float[] otherAttrPassiveEffects,
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

        public void PassiveEffectChangeTrap(float[] trapAdd,
            (float TrapAtkMulti, float TrapSurvivalMulti) trapBaseAttr)
        {
            BattleUnitMoverStandard.PassiveEffectChangeTrap(trapAdd, trapBaseAttr, this);
        }


        public IMapInteractable PutInteractable(TwoDPoint pos, bool isActive)
        {
            return new VehicleCanIn(this, pos);
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

        public ImmutableArray<IActResult> ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            return interactive switch
            {
                MapInteract.InVehicleCall => new IActResult[]
                    {new DropThings(characterStatus.GetInAVehicle(this))}.ToImmutableArray(),
                MapInteract.KickVehicleCall => new IActResult[]
                    {new DropThings(KickBySomeBody(characterStatus.GetPos()))}.ToImmutableArray(),
                _ => ImmutableArray<IActResult>.Empty
            };
        }

        public int GetId()
        {
            return (int) VId;
        }

        public int GetNum()
        {
            return 1;
        }

        private IEnumerable<IMapInteractable> KickBySomeBody(TwoDPoint pos)
        {
            var opos = InWhichMapInteractive == null ? pos : InWhichMapInteractive.GetAnchor().GetMid(pos);

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


        private void RefreshByPass(Dictionary<passive_id, PassiveTrait> characterStatusPassiveTraits)
        {
            var passiveTraits =
                characterStatusPassiveTraits.Values.Where(x => x.PassiveTraitEffect
                    is IPassiveTraitEffectForVehicle);
            var groupBy = passiveTraits.GroupBy(x => x.GetType());
            foreach (var grouping in groupBy)
            {
                var firstOrDefault = grouping.FirstOrDefault();

                if (firstOrDefault?.PassiveTraitEffect is IPassiveTraitEffectForVehicle passiveTraitEffect)
                {
                    var aggregate = grouping.Aggregate(new float[] { },
                        (s, x) => s.Plus(x.PassiveTraitEffect.GenEffect(x.Level).GetVector()));

                    if (!aggregate.Any())
                    {
                        continue;
                    }

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

        public void TrapAboutRefresh(float[] trapAdd)
        {
            BattleUnitMoverStandard.TrapAboutRefresh(trapAdd, this);
        }

        public void RegenStatusRefresh(float[] rPassiveEffects)
        {
            BattleUnitMoverStandard.RegenStatusRefresh(rPassiveEffects, this);
        }


        public void AbsorbDamage(uint genDamageShardedDamage, uint genDamageShardedNum, uint shardedDamage)
        {
            SurvivalStatus.AbsorbDamage(genDamageShardedDamage, genDamageShardedNum, AbsorbStatus, shardedDamage);
        }

        public void AbsorbStatusRefresh(float[] vector)
        {
            BattleUnitMoverStandard.AbsorbStatusRefresh(vector, this);
        }
    }
}