using System.Linq;
using game_stuff;

namespace rogue_game
{
    public readonly struct PlayerSave
    {
        public int[] WeaponIds { get; }

        public int BaseAttrId { get; }

        public int VehicleId { get; }

        public int[] VWeaponIds { get; }

        public GameItem[] BagData { get; }

        public int[][] PassiveData { get; }

        public int PropId { get; }

        public int NowHp { get; }

        public int NowArmor { get; }

        public int NowAmmo { get; }

        public int VNowHp { get; }

        public int VNowArmor { get; }

        public int VNowAmmo { get; }

        private PlayerSave(int[] weaponIds, int baseAttrId, int vehicleId, int[] vWeaponIds, GameItem[] bagData,
            int[][] passiveData, int propId, int nowHp, int nowArmor, int nowAmmo, int vNowHp, int vNowArmor,
            int vNowAmmo)
        {
            WeaponIds = weaponIds;
            BaseAttrId = baseAttrId;
            VehicleId = vehicleId;
            VWeaponIds = vWeaponIds;
            BagData = bagData;
            PassiveData = passiveData;
            PropId = propId;
            NowHp = nowHp;
            NowArmor = nowArmor;
            NowAmmo = nowAmmo;
            VNowHp = vNowHp;
            VNowArmor = vNowArmor;
            VNowAmmo = vNowAmmo;
        }

        public static PlayerSave GenPlayerSave(CharacterStatus characterStatus)
        {
            var weaponIds = characterStatus.Weapons.Keys.ToArray();
            var keyValuePairs = characterStatus.PassiveTraits.ToDictionary(p => p.Key, pair => (int) pair.Value.Level)
                .Select(x => new[] {x.Key, x.Value}).ToArray();
            var passiveData = keyValuePairs;
            var baseAttrId = characterStatus.BaseAttrId;
            var characterStatusNowVehicle = characterStatus.NowVehicle;
            var vehicleId = characterStatusNowVehicle?.VId ?? -1;
            var propId = characterStatus.Prop?.PId ?? -1;
            var characterStatusSurvivalStatus = characterStatus.SurvivalStatus;
            var nowHp = (int) characterStatusSurvivalStatus.NowHp;
            var nowArmor = (int) characterStatusSurvivalStatus.NowArmor;
            var nowAmmo = characterStatus.NowAmmo;
            var nowVehicleSurvivalStatus = characterStatusNowVehicle?.SurvivalStatus;
            var vNowHp = (int) nowVehicleSurvivalStatus?.NowHp ?? -1;
            var vNowArmor = (int) nowVehicleSurvivalStatus?.NowArmor ?? -1;
            var vNowAmmo = characterStatusNowVehicle?.NowAmmo ?? -1;
            var vWeaponIds = characterStatusNowVehicle?.Weapons.Keys.ToArray();
            var bagData = characterStatus.PlayingItemBag.GameItems.Select(p => new GameItem(p.Key, p.Value)).ToArray();

            return new PlayerSave(weaponIds, baseAttrId, vehicleId, vWeaponIds, bagData, passiveData, propId, nowHp,
                nowArmor,
                nowAmmo, vNowHp, vNowArmor, vNowAmmo);
        }
    }
}