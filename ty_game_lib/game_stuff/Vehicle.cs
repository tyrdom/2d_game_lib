using System.Collections.Generic;

namespace game_stuff
{
    public class Vehicle
    {
        public Vehicle(BodySize vehicleSize, float vehicleMaxMoveSpeed, float vehicleMinMoveSpeed,
            float vehicleAddMoveSpeed, Scope vehicleScope, Dictionary<int, Weapon> vehicleWeapons)
        {
            VehicleSize = vehicleSize;
            VehicleMaxMoveSpeed = vehicleMaxMoveSpeed;
            VehicleMinMoveSpeed = vehicleMinMoveSpeed;
            VehicleAddMoveSpeed = vehicleAddMoveSpeed;
            VehicleScope = vehicleScope;
            VehicleWeapons = vehicleWeapons;
            WhoDrive = null;
        }

        public CharacterStatus? WhoDrive { get; set; }
        public BodySize VehicleSize { get; }
        private float VehicleMaxMoveSpeed { get; }
        private float VehicleMinMoveSpeed { get; }
        private float VehicleAddMoveSpeed { get; }
        private Scope VehicleScope { get; }
        private Dictionary<int, Weapon> VehicleWeapons { get; set; }
    }
}