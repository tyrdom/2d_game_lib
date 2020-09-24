using System.Collections.Generic;

namespace game_stuff
{
    public class Vehicle
    {
        public Vehicle(BodySize vehicleSize, float vehicleMaxMoveSpeed, float vehicleMinMoveSpeed,
            float vehicleAddMoveSpeed, Scope vehicleScope, Dictionary<int, Weapon> weapons, Bullet destroyBullet,
            DamageHealStatus damageHealStatus, int destroyTick, int weaponSlot, int getInTick, int nowAmmo, int maxAmmo)
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
        public DamageHealStatus DamageHealStatus { get; }

        public int GetInTick { get; }
    }
}