using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Vehicle : ICanPutInMapInteractable, IBattleUnit
    {
        public Vehicle(BodySize size, float maxMoveSpeed, float minMoveSpeed,
            float addMoveSpeed, Scope scope, Dictionary<int, Weapon> weapons, Bullet destroyBullet,
            SurvivalStatus survivalStatus, int destroyTick, int weaponCarryMax, int getInTick, int nowAmmo,
            int maxAmmo,
            Skill outAct, AttackStatus attackStatus, base_attr_id baseAttrId)
        {
            Size = size;
            MaxMoveSpeed = maxMoveSpeed;
            MinMoveSpeed = minMoveSpeed;
            AddMoveSpeed = addMoveSpeed;
            Scope = scope;
            Weapons = weapons;
            DestroyBullet = destroyBullet;
            SurvivalStatus = survivalStatus;
            DestroyTick = destroyTick;
            WeaponCarryMax = weaponCarryMax;
            GetInTick = getInTick;
            NowAmmo = nowAmmo;
            MaxAmmo = maxAmmo;
            OutAct = outAct;
            AttackStatus = attackStatus;
            BaseAttrId = baseAttrId;
            WhoDrive = null;
        }

        public CharacterStatus? WhoDrive { get; set; }
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
        private int DestroyTick { get; }

        private Bullet DestroyBullet { get; }
        public SurvivalStatus SurvivalStatus { get; }

        public void SurvivalStatusRefresh(IEnumerable<SurvivalAboutPassiveEffect> survivalAboutPassiveEffects)
        {
            BattleUnitStandard.SurvivalStatusRefresh(survivalAboutPassiveEffects,this);
        }

        public AttackStatus AttackStatus { get; }

        public void AttackStatusRefresh(IEnumerable<AtkAboutPassiveEffect> atkAboutPassiveEffects)
        {
            BattleUnitStandard.AtkStatusRefresh(atkAboutPassiveEffects, this);
        }

        public Skill OutAct { get; }
        private int GetInTick { get; }

        public void AddAmmo(int ammoAdd) => NowAmmo = Math.Min(MaxAmmo, NowAmmo + ammoAdd);


        public IMapInteractable? InWhichMapInteractive { get; set; }

        public IMapInteractable GenIMapInteractable(TwoDPoint pos)
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
            return characterStatus.NowVehicle == null;
        }

        public bool CanInterActTwoBy(CharacterStatus characterStatus)
        {
            return true;
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

            var mapIntractable = Weapons.Select(x => x.Value.GenIMapInteractable(opos));
            Weapons.Clear();
            return mapIntractable;
        }
    }
}