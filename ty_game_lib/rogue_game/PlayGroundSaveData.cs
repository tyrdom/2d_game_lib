using System;
using System.Collections.Generic;
using System.Linq;
using game_config;
using game_stuff;
using rogue_chapter_maker;

namespace rogue_game
{
    [Serializable]
    public class PlayGroundSaveData
    {
        public int ResId { get; }

        public PlayGroundSaveData(SaleUnitSave[] saleUnitSaves, int mGid, int resId)
        {
            SaleUnitSaves = saleUnitSaves;
            MGid = mGid;
            ResId = resId;
        }

        public static PlayGroundSaveData GroundSaveData(PlayGround playGround)
        {
            var saleUnits = playGround.GetMapApplyDevices()
                .Select(x => x.CharActOne.InMapInteractable).OfType<SaleUnit>();
            var saleUnitSaves = saleUnits.Select(SaleUnitSave.GenByASaleUnit);
            var playGroundMgId = playGround.MgId;
            var playGroundResMId = playGround.ResMId;
            var playGroundSaveData = new PlayGroundSaveData(saleUnitSaves.ToArray(), playGroundMgId, playGroundResMId);
            return playGroundSaveData;
        }

        public int MGid { get; }
        public SaleUnitSave[] SaleUnitSaves { get; }
    }

    [Serializable]
    public class SaleStuffSave
    {
        public SaleStuffSave(ContainType containType, int id, int num)
        {
            ContainType = containType;
            Id = id;
            Num = num;
        }

        private ContainType ContainType { get; }

        private int Id { get; }

        private int Num { get; }

        public static SaleStuffSave StuffSave(ISaleStuff stuff)
        {
            var id = stuff.GetId();
            var num = stuff.GetNum();
            var containType = stuff.GetTitle();
            return new SaleStuffSave(containType, id, num);
        }

        public IEnumerable<ISaleStuff> GenStuff()
        {
            switch (ContainType)
            {
                case ContainType.PassiveC:
                    var passiveId = (passive_id) Id;
                    var genManyByPId = PassiveTrait.GenManyByPId(passiveId, (uint) Num).OfType<ISaleStuff>();
                    return genManyByPId;
                case ContainType.PropC:
                    var propId = (prop_id) Id;
                    var genById = Prop.GenById(propId);
                    return new ISaleStuff[] {genById};
                case ContainType.WeaponC:
                    var weaponId = (weapon_id) Id;
                    var byId = Weapon.GenById(weaponId);
                    return new ISaleStuff[] {byId};
                case ContainType.VehicleC:
                    var vehicleId = (vehicle_id) Id;
                    var vehicle = Vehicle.GenById(vehicleId);
                    return new ISaleStuff[] {vehicle};
                case ContainType.GameItemC:
                    var itemId = (item_id) Id;
                    var gameItem = new GameItem(itemId, Num);
                    return new ISaleStuff[] {gameItem};
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [Serializable]
    public class SaleUnitSave
    {
        public SaleUnitSave(IEnumerable<ISaleStuff> saleItems, int stack, GameItem salePrize,
            Dictionary<int, int> doneDictionary)
        {
            Good = saleItems.Select(SaleStuffSave.StuffSave).ToArray();
            Stack = stack;
            Cost = salePrize;
            DoneDictionary = doneDictionary;
        }


        private SaleStuffSave[] Good { get; }

        private int Stack { get; }

        private Dictionary<int, int> DoneDictionary { get; }
        private GameItem Cost { get; }

        public static SaleUnitSave GenByASaleUnit(SaleUnit saleUnit)
        {
            var saleUnitCost = saleUnit.Cost;
            var saleUnitGood = saleUnit.Good;
            var restStack = saleUnit.Stack;
            var saleUnitDoneDictionary = saleUnit.DoneDictionary;
            return new SaleUnitSave(saleUnitGood, restStack, saleUnitCost, saleUnitDoneDictionary);
        }

        public SaleUnit LoadToSaleUnit()
        {
            var saleStuffs = Good.SelectMany(x => x.GenStuff()).ToArray();

            return new SaleUnit(null, Cost, saleStuffs, Stack, new GameItem[] { }, DoneDictionary);
        }
    }
}