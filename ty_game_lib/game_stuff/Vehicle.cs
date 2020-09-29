using System;
using System.Collections.Generic;
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

        public int NowAmmo { get; set; }

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
            return GameTools.GenIMapInteractable(pos, InWhichMapInteractive, this);
        }

        public bool CanPick(CharacterStatus characterStatus)
        {
            return characterStatus.NowVehicle == null;
        }
    }
}