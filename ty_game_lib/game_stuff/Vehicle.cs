using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Vehicle : ISaleStuff, IMoveBattleAttrModel, ICanDrop
    {
        public static Vehicle GenByConfig(vehicle vehicle)
        {
            var bodySize = TempConfig.GetBodySize(vehicle.BodyId);
            var tickByTime = TempConfig.GetTickByTime(vehicle.DestoryDelayTime);

            var genByBulletId = Bullet.GenByBulletId(vehicle.DestoryBullet);
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
                vehicle.MaxWeaponSlot, genSkillById, vehicle.AttrId);
        }

        public Vehicle(BodySize size, Scope scope, Dictionary<int, Weapon> weapons, Bullet destroyBullet,
            uint destroyTick, int weaponCarryMax,
            Skill outAct, base_attr_id baseAttrId)
        {
            var (max, min, add, maxAmmo, _, attackStatus, survivalStatus) =
                CharacterStatus.GenAttrBaseByConfig(baseAttrId);

            Size = size;
            MaxMoveSpeed = max;
            MinMoveSpeed = min;
            AddMoveSpeed = add;
            Scope = scope;
            Weapons = weapons;
            DestroyBullet = destroyBullet;
            SurvivalStatus = survivalStatus;
            DestroyTick = destroyTick;
            WeaponCarryMax = weaponCarryMax;
            NowAmmo = 0;
            MaxAmmo = maxAmmo;
            OutAct = outAct;
            AttackStatus = attackStatus;
            BaseAttrId = baseAttrId;
            WhoDriveOrCanDrive = null;
        }

        public CharacterStatus? WhoDriveOrCanDrive { get; set; }
        public BodySize Size { get; }

        public float MaxMoveSpeed { get; }
        public float MinMoveSpeed { get; }
        public float AddMoveSpeed { get; }
        public base_attr_id BaseAttrId { get; }
        public Scope Scope { get; }
        public Dictionary<int, Weapon> Weapons { get; }

        public int NowAmmo { get; private set; }

        private int MaxAmmo { get; }
        public int WeaponCarryMax { get; }
        private uint DestroyTick { get; }

        private Bullet DestroyBullet { get; }
        public SurvivalStatus SurvivalStatus { get; }

        public void SurvivalStatusRefresh(Vector<float> survivalAboutPassiveEffects)
        {
            BattleUnitStandard.SurvivalStatusRefresh(survivalAboutPassiveEffects, this);
        }

        public AttackStatus AttackStatus { get; }

        public void AttackStatusRefresh(Vector<float> atkAboutPassiveEffects)
        {
            BattleUnitStandard.AtkStatusRefresh(atkAboutPassiveEffects, this);
        }

        public void OtherStatusRefresh(Vector<float> otherAttrPassiveEffects)
        {
            BattleUnitStandard.OtherStatusRefresh(otherAttrPassiveEffects, this);
        }

        public Skill OutAct { get; }

        public void AddAmmo(int ammoAdd) => NowAmmo = Math.Min(MaxAmmo, NowAmmo + ammoAdd);

        public void PassiveEffectChangeOther(Vector<float> otherAttrPassiveEffects,
            (int MaxAmmo, float MoveMaxSpeed, float MoveMinSpeed, float MoveAddSpeed, int StandardPropMaxStack, float
                RecycleMulti) otherBaseStatus)
        {
            throw new NotImplementedException();
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

        public IEnumerable<IMapInteractable> ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            return interactive switch
            {
                MapInteract.InVehicleCall => characterStatus.GetInAVehicle(this),
                MapInteract.KickVehicleCall => KickBySomeBody(characterStatus.GetPos()),
                _ => throw new ArgumentOutOfRangeException(nameof(interactive), interactive, null)
            };
        }

        private IEnumerable<IMapInteractable> KickBySomeBody(TwoDPoint pos)
        {
            var opos = InWhichMapInteractive == null ? pos : InWhichMapInteractive.GetPos().GetMid(pos);

            var mapIntractable = Weapons.Select(x => x.Value.DropAsIMapInteractable(opos));
            Weapons.Clear();
            return mapIntractable;
        }

        public void FullAmmo()
        {
            NowAmmo = MaxAmmo;
        }
    }
}