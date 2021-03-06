using System;
using System.Linq;
using System.Runtime.CompilerServices;
using collision_and_rigid;
using game_config;
using game_stuff;

namespace rogue_game
{
    [Serializable]
    public class PlayerStatusSave
    {
        public int TeamId { get; set; }
        public size BodySize { get; set; }
        public weapon_id[] WeaponIds { get; set; }

        public int BaseAttrId { get; set; }

        public int VehicleId { get; set; }

        public weapon_id[] VWeaponIds { get; set; }

        public GameItem[] BagData { get; set; }

        public int[][] PassiveData { get; set; }

        public int PropId { get; set; }

        public int PropV { get; set; }
        public int NowHp { get; set; }

        public int NowArmor { get; set; }

        public int NowAmmo { get; set; }

        public int VNowHp { get; set; }

        public int VNowArmor { get; set; }

        public int VNowAmmo { get; set; }


        // private void Save(weapon_id[] weaponIds, int baseAttrId, int vehicleId, weapon_id[] vWeaponIds,
        //     GameItem[] bagData,
        //     int[][] passiveData, int propId, int nowHp, int nowArmor, int nowAmmo, int vNowHp, int vNowArmor,
        //     int vNowAmmo, int mapGid)
        // {
        //     WeaponIds = weaponIds;
        //     BaseAttrId = baseAttrId;
        //     VehicleId = vehicleId;
        //     VWeaponIds = vWeaponIds;
        //     BagData = bagData;
        //     PassiveData = passiveData;
        //     PropId = propId;
        //     NowHp = nowHp;
        //     NowArmor = nowArmor;
        //     NowAmmo = nowAmmo;
        //     VNowHp = vNowHp;
        //     VNowArmor = vNowArmor;
        //     VNowAmmo = vNowAmmo;
        // }

        private PlayerStatusSave(weapon_id[] weaponIds, int baseAttrId, int vehicleId, weapon_id[] vWeaponIds,
            GameItem[] bagData,
            int[][] passiveData, int propId, int propV, int nowHp, int nowArmor, int nowAmmo, int vNowHp, int vNowArmor,
            int vNowAmmo, size size, int teamId)
        {
            BodySize = size;
            TeamId = teamId;
            WeaponIds = weaponIds;
            BaseAttrId = baseAttrId;
            VehicleId = vehicleId;
            VWeaponIds = vWeaponIds;
            BagData = bagData;
            PassiveData = passiveData;
            PropId = propId;
            PropV = propV;
            NowHp = nowHp;
            NowArmor = nowArmor;
            NowAmmo = nowAmmo;
            VNowHp = vNowHp;
            VNowArmor = vNowArmor;
            VNowAmmo = vNowAmmo;
        }


        public CharacterBody LoadBody(int gid)
        {
            var dictionary = BagData.ToDictionary(item => item.ItemId, i => i.Num);
            var playingItemBag = new PlayingItemBag(dictionary);


            var passiveTraits = PassiveData.Select(x => PassiveTrait.GenById((passive_id) x[0], (uint) x[1]))
                .ToDictionary(x => x.PassId, x => x);

#if DEBUG
            Console.Out.WriteLine("pass load");
            foreach (var keyValuePair in passiveTraits)
            {
                Console.Out.WriteLine($"{keyValuePair.Key} : {keyValuePair.Value.Level}");
            }
#endif
            var characterStatus =
                new CharacterStatus(gid, BaseAttrId, playingItemBag, passiveTraits);
            var characterBody = new CharacterBody(TwoDPoint.Zero(), BodySize, characterStatus, TwoDPoint.Zero(),
                AngleSight.StandardAngleSight(),
                TeamId);
            var enumerable = WeaponIds.Select(Weapon.GenById);
            foreach (var weapon in enumerable)
            {
                characterStatus.PicAWeapon(weapon);
            }

            if (PropId >= 0)
            {
                var byId = Prop.GenById((prop_id) PropId);
                characterStatus.PickAProp(byId);
#if DEBUG
                Console.Out.WriteLine($"prop load {byId.PId}");

#endif
            }

            characterStatus.SetPropPoint(PropV);
#if DEBUG
            Console.Out.WriteLine($"prop point load {PropV}");
#endif
            characterStatus.NowAmmo = NowAmmo;
            characterStatus.SurvivalStatus.SetArmor(NowArmor);
            characterStatus.SurvivalStatus.SetHp(NowHp);
            if (VehicleId < 0) return characterBody;
            {
                var genById = Vehicle.GenById((vehicle_id) VehicleId);
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

            return characterBody;
        }

        // public void Save(CharacterStatus characterStatus, int mapGid)
        // {
        //     var weaponIds = characterStatus.Weapons.Values.Select(x => x.WId).ToArray();
        //     var keyValuePairs = characterStatus.PassiveTraits
        //         .ToDictionary(p => p.Key, pair => (int) pair.Value.Level)
        //         .Select(x => new[] {(int) x.Key, x.Value}).ToArray();
        //
        //     var baseAttrId = characterStatus.BaseAttrId;
        //     var characterStatusNowVehicle = characterStatus.NowVehicle;
        //     var vehicleId = (int?) (characterStatusNowVehicle?.VId) ?? -1;
        //     var propId = (int?) characterStatus.Prop?.PId ?? -1;
        //     var characterStatusSurvivalStatus = characterStatus.SurvivalStatus;
        //     var nowHp = (int) characterStatusSurvivalStatus.NowHp;
        //     var nowArmor = (int) characterStatusSurvivalStatus.NowArmor;
        //     var nowAmmo = characterStatus.NowAmmo;
        //     var nowVehicleSurvivalStatus = characterStatusNowVehicle?.SurvivalStatus;
        //     var vNowHp = (int?) nowVehicleSurvivalStatus?.NowHp ?? -1;
        //     var vNowArmor = (int?) nowVehicleSurvivalStatus?.NowArmor ?? -1;
        //     var vNowAmmo = characterStatusNowVehicle?.NowAmmo ?? -1;
        //     var vWeaponIds = characterStatusNowVehicle?.Weapons.Values.Select(x => x.WId).ToArray() ??
        //                      new weapon_id[] { };
        //     var bagData = characterStatus.PlayingItemBag.GameItems.Select(p => new GameItem(p.Key, p.Value)).ToArray();
        //
        //     Save(weaponIds, baseAttrId, vehicleId, vWeaponIds, bagData, keyValuePairs, propId, nowHp,
        //         nowArmor,
        //         nowAmmo, vNowHp, vNowArmor, vNowAmmo, mapGid);
        // }


        public static PlayerStatusSave GenSave(CharacterStatus characterStatus, int teamId, size size)
        {
            var weaponIds = characterStatus.Weapons.Values.Select(x => x.WId).ToArray();
            var keyValuePairs = characterStatus.PassiveTraits.ToDictionary(p => p.Key, pair => (int) pair.Value.Level)
                .Select(x => new[] {(int) x.Key, x.Value}).ToArray();
            var baseAttrId = characterStatus.BaseAttrId;
#if DEBUG
            var aggregate = keyValuePairs.Aggregate("", (s, x) => s + x[0] + ":" + x[1] + "\n");
            Console.Out.WriteLine($"pass save {aggregate}");
#endif
            var characterStatusNowVehicle = characterStatus.NowVehicle;
            var vehicleId = (int?) (characterStatusNowVehicle?.VId) ?? -1;
            var propId = (int?) characterStatus.Prop?.PId ?? -1;
            var characterStatusNowPropPoint =
                characterStatus.NowPropPoint;
#if DEBUG
            Console.Out.WriteLine($"save prop {characterStatus.Prop?.PId}: {propId}, {characterStatusNowPropPoint}");
#endif
            var characterStatusSurvivalStatus = characterStatus.SurvivalStatus;

            var nowHp = (int) characterStatusSurvivalStatus.NowHp;
            var nowArmor = (int) characterStatusSurvivalStatus.NowArmor;
            var nowAmmo = characterStatus.NowAmmo;
            var nowVehicleSurvivalStatus = characterStatusNowVehicle?.SurvivalStatus;
            var vNowHp = (int?) nowVehicleSurvivalStatus?.NowHp ?? -1;
            var vNowArmor = (int?) nowVehicleSurvivalStatus?.NowArmor ?? -1;
            var vNowAmmo = characterStatusNowVehicle?.NowAmmo ?? -1;
            var vWeaponIds = characterStatusNowVehicle?.Weapons.Values.Select(x => x.WId).ToArray() ??
                             new weapon_id[] { };
            var bagData = characterStatus.PlayingItemBag.GameItems.Select(p => new GameItem(p.Key, p.Value)).ToArray();

            return new PlayerStatusSave(weaponIds, baseAttrId, vehicleId, vWeaponIds, bagData, keyValuePairs, propId,
                characterStatusNowPropPoint,
                nowHp,
                nowArmor,
                nowAmmo, vNowHp, vNowArmor, vNowAmmo, size, teamId);
        }
    }
}