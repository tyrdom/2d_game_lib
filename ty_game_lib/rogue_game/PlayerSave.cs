using System.Linq;
using System.Runtime.CompilerServices;
using game_stuff;

namespace rogue_game
{
    public struct PlayerSave
    {
        public int[] WeaponIds { get; set; }

        public int BaseAttrId { get; set; }

        public int VehicleId { get; set; }

        public int[] VWeaponIds { get; set; }

        public GameItem[] BagData { get; set; }

        public int[][] PassiveData { get; set; }

        public int PropId { get; set; }

        public int NowHp { get; set; }

        public int NowArmor { get; set; }

        public int NowAmmo { get; set; }

        public int VNowHp { get; set; }

        public int VNowArmor { get; set; }

        public int VNowAmmo { get; set; }

        public int MapGid { get; set; }

        private void Save(int[] weaponIds, int baseAttrId, int vehicleId, int[] vWeaponIds, GameItem[] bagData,
            int[][] passiveData, int propId, int nowHp, int nowArmor, int nowAmmo, int vNowHp, int vNowArmor,
            int vNowAmmo, int mapGid)
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
            MapGid = mapGid;
        }

        private PlayerSave(int[] weaponIds, int baseAttrId, int vehicleId, int[] vWeaponIds, GameItem[] bagData,
            int[][] passiveData, int propId, int nowHp, int nowArmor, int nowAmmo, int vNowHp, int vNowArmor,
            int vNowAmmo, int mapGid)
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
            MapGid = mapGid;
        }

        public int GetMGid()
        {
            return MapGid;
        }
        public CharacterStatus LoadSaveStatus(int gid)
        {
            var dictionary = BagData.ToDictionary(item => item.ItemId, i => i.Num);
            var playingItemBag = new PlayingItemBag(dictionary);


            var passiveTraits = PassiveData.ToDictionary(x => x[0], x => new PassiveTrait(
                x[0], (uint) x[1], PassiveEffectStandard.GenById(x[0])));
            var characterStatus =
                new CharacterStatus(gid, BaseAttrId, playingItemBag, RogueGame.GetLevelUp(), passiveTraits);
            var enumerable = WeaponIds.Select(Weapon.GenById);
            foreach (var weapon in enumerable)
            {
                characterStatus.PicAWeapon(weapon);
            }

            var byId = Prop.GenById(PropId);
            characterStatus.PickAProp(byId);
            characterStatus.NowAmmo = NowAmmo;
            characterStatus.SurvivalStatus.SetArmor(NowArmor);
            characterStatus.SurvivalStatus.SetHp(NowHp);
            if (VehicleId < 0) return characterStatus;
            {
                var genById = Vehicle.GenById(VehicleId);
                characterStatus.GetInAVehicle(genById);

                var weapons = VWeaponIds.Select(Weapon.GenById);
                foreach (var weapon in weapons)
                {
                    characterStatus.PicAWeapon(weapon);
                }

                var characterStatusNowVehicle = characterStatus.NowVehicle;
                characterStatusNowVehicle?.SurvivalStatus.SetHp(VNowHp);
                characterStatusNowVehicle?.SurvivalStatus.SetArmor(VNowArmor);
                characterStatusNowVehicle?.SetAmmo(VNowAmmo);
            }

            return characterStatus;
        }

        public void Save(CharacterStatus characterStatus, int mapGid)
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
            var vNowHp = (int?) nowVehicleSurvivalStatus?.NowHp ?? -1;
            var vNowArmor = (int?) nowVehicleSurvivalStatus?.NowArmor ?? -1;
            var vNowAmmo = characterStatusNowVehicle?.NowAmmo ?? -1;
            var vWeaponIds = characterStatusNowVehicle?.Weapons.Keys.ToArray() ?? new int[] { };
            var bagData = characterStatus.PlayingItemBag.GameItems.Select(p => new GameItem(p.Key, p.Value)).ToArray();

            Save(weaponIds, baseAttrId, vehicleId, vWeaponIds, bagData, passiveData, propId, nowHp,
                nowArmor,
                nowAmmo, vNowHp, vNowArmor, vNowAmmo, mapGid);
        }

        public static PlayerSave GenPlayerSave(CharacterStatus characterStatus, int mapGid)
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
            var vNowHp = (int?) nowVehicleSurvivalStatus?.NowHp ?? -1;
            var vNowArmor = (int?) nowVehicleSurvivalStatus?.NowArmor ?? -1;
            var vNowAmmo = characterStatusNowVehicle?.NowAmmo ?? -1;
            var vWeaponIds = characterStatusNowVehicle?.Weapons.Keys.ToArray() ?? new int[] { };
            var bagData = characterStatus.PlayingItemBag.GameItems.Select(p => new GameItem(p.Key, p.Value)).ToArray();

            return new PlayerSave(weaponIds, baseAttrId, vehicleId, vWeaponIds, bagData, passiveData, propId, nowHp,
                nowArmor,
                nowAmmo, vNowHp, vNowArmor, vNowAmmo, mapGid);
        }
    }
}