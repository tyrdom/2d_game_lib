using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;
using game_config;
using static game_stuff.PassiveTrait;

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

        public ISaleStuff? GetTitleGood()
        {
            var firstOrDefault = Good.FirstOrDefault(x =>
            {
                return x switch
                {
                    PassiveTrait passiveTrait => passiveTrait.CanTitle(),
                    _ => true
                };
            });
            return firstOrDefault;
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
            var saleStuff = saleUnit.SaleType switch
            {
                sale_type.prop => new ISaleStuff[] {Prop.GenById(chooseRandCanSame)},
                sale_type.passive => GenManyById(chooseRandCanSame, 1).OfType<ISaleStuff>().ToArray(),
                _ => throw new ArgumentOutOfRangeException()
            };

            var gameItems = new GameItem[] { };
            var unit = new SaleUnit(null, genByConfigGain, saleStuff, saleUnit.Stack,
                gameItems);
            return unit;
        }
    }
}