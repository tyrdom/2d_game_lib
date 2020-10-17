using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class Vehicle : ICanPutInCage
    {
        public Vehicle(BodySize vehicleSize, float vehicleMaxMoveSpeed, float vehicleMinMoveSpeed,
            float vehicleAddMoveSpeed, Scope vehicleScope, Dictionary<int, Weapon> weapons, Bullet destroyBullet,
            DamageHealStatus damageHealStatus, int destroyTick, int weaponCarryMax, int getInTick, int nowAmmo,
            int maxAmmo,
            Skill outAct)
        {
            VehicleSize = vehicleSize;
            VehicleMaxMoveSpeed = vehicleMaxMoveSpeed;
            VehicleMinMoveSpeed = vehicleMinMoveSpeed;
            VehicleAddMoveSpeed = vehicleAddMoveSpeed;
            VehicleScope = vehicleScope;
            Weapons = weapons;
            DestroyBullet = destroyBullet;
            DamageHealStatus = damageHealStatus;
            DestroyTick = destroyTick;
            WeaponCarryMax = weaponCarryMax;
            GetInTick = getInTick;
            NowAmmo = nowAmmo;
            MaxAmmo = maxAmmo;
            OutAct = outAct;
            WhoDrive = null;
        }

        public CharacterStatus? WhoDrive { get; set; }
        public BodySize VehicleSize { get; }
        private float VehicleMaxMoveSpeed { get; }
        private float VehicleMinMoveSpeed { get; }
        private float VehicleAddMoveSpeed { get; }
        private Scope VehicleScope { get; }
        public Dictionary<int, Weapon> Weapons { get; }

        public int NowAmmo { get; private set; }

        private int MaxAmmo { get; }
        public int WeaponCarryMax { get; }
        private int DestroyTick { get; }

        private Bullet DestroyBullet { get; }
        private DamageHealStatus DamageHealStatus { get; }

        public Skill OutAct { get; }
        private int GetInTick { get; }

        public void AddAmmo(int ammoAdd) => NowAmmo = Math.Min(MaxAmmo, NowAmmo + ammoAdd);


        public IMapInteractable? InWhichMapInteractive { get; set; }

        public IMapInteractable GenIMapInteractable(TwoDPoint pos)
        {
            if (InWhichMapInteractive == null)
            {
                return new VehicleCanIn(this, pos);
            }

            InWhichMapInteractive.ReLocate(pos);
            return InWhichMapInteractive;
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