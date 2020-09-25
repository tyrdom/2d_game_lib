using System;
using System.Collections.Generic;

namespace game_stuff
{
    public class Vehicle : ICanPutInCage
    {
        public Vehicle(BodySize vehicleSize, float vehicleMaxMoveSpeed, float vehicleMinMoveSpeed,
            float vehicleAddMoveSpeed, Scope vehicleScope, Dictionary<int, Weapon> weapons, Bullet destroyBullet,
            DamageHealStatus damageHealStatus, int destroyTick, int weaponSlot, int getInTick, int nowAmmo, int maxAmmo,
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
            WeaponSlot = weaponSlot;
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

        private int NowAmmo { get; set; }

        private int MaxAmmo { get; }
        private int WeaponSlot { get; }
        private int DestroyTick { get; }

        private Bullet DestroyBullet { get; }
        private DamageHealStatus DamageHealStatus { get; }

        public Skill OutAct { get; }
        private int GetInTick { get; }

        public void AddAmmo(int ammoAdd) => NowAmmo = Math.Min(MaxAmmo, NowAmmo + ammoAdd);

        public void BePickCage()
        {
            WhoDrive = null;
            //todo
        }
    }
}