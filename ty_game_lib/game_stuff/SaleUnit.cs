using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class SaleUnit : ISaleUnit
    {
        public SaleUnit(IMapInteractable? inWhichMapInteractive, GameItem cost, ISaleStuff[] good,
            int stack, GameItem[] orCosts)
        {
            InWhichMapInteractive = inWhichMapInteractive;
            Cost = cost;
            Good = good;
            Stack = stack;
            OrCosts = orCosts;
            DoneDictionary = new Dictionary<int, int>();
        }


        public IMapInteractable PutInteractable(TwoDPoint pos, bool isActive)
        {
            return new ApplyDevice(this, pos, isActive);
        }

        public IMapInteractable? InWhichMapInteractive { get; set; }

        public GameItem Cost { get; }
        public GameItem[] OrCosts { get; }

        public ISaleStuff[] GetGood()
        {
            return Good;
        }

        public int GetRestStack(int? gid)
        {
            return SaleUnitStandard.GetRestStack(gid, this);
        }

        public ISaleStuff[] Good { get; }

        public Dictionary<int, int> DoneDictionary { get; }

        public int Stack { get; }

        public bool CanInterActOneBy(CharacterStatus characterStatus)
        {
            return SaleUnitStandard.CanInterActOneBy(characterStatus, this);
        }

        public bool CanInterActTwoBy(CharacterStatus characterStatus)
        {
            return SaleUnitStandard.CanInterActTwoBy(characterStatus, this);
        }

        public ImmutableArray<IActResult> ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            return SaleUnitStandard.ActWhichChar(characterStatus, interactive, this);
        }

        public static ISaleUnit GenByConfig(sale_unit saleUnit)
        {
            var firstOrDefault = saleUnit.Cost.FirstOrDefault();
            var genByConfigGain = firstOrDefault == null
                ? new GameItem(item_id.coin, 0)
                : GameItem.GenByConfigGain(firstOrDefault);
            var chooseRandCanSame = saleUnit.LimitIdRange.ChooseRandOne();
            ISaleStuff saleStuff = saleUnit.SaleType switch
            {
                sale_type.prop => Prop.GenById(chooseRandCanSame),
                sale_type.passive => PassiveTrait.GenById(chooseRandCanSame, 1),
                _ => throw new ArgumentOutOfRangeException()
            };

            var gameItems = new GameItem[] { };
            var unit = new SaleUnit(null, genByConfigGain, new[] {saleStuff}, saleUnit.Stack,
                gameItems);
            return unit;
        }
    }
}