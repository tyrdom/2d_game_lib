using System;
using System.Collections.Generic;

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

        public IEnumerable<IMapInteractable> ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            return SaleUnitStandard.ActWhichChar(characterStatus, interactive, this);
        }
    }
}