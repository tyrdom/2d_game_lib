using System;
using System.Collections.Generic;

namespace game_stuff
{
    public class SaleUnit : ISaleUnit
    {
        public SaleUnit(IMapInteractable? inWhichMapInteractive, GameItem cost, ISaleStuff good,
            int stack)
        {
            InWhichMapInteractive = inWhichMapInteractive;
            Cost = cost;
            Good = good;
            Stack = stack;
            DoneDictionary = new Dictionary<CharacterStatus, int>();
        }


      

        public IMapInteractable? InWhichMapInteractive { get; set; }

        public GameItem Cost { get; }

        public ISaleStuff GetGood()
        {
            return Good;
        }

        public ISaleStuff Good { get; }

        public Dictionary<CharacterStatus, int> DoneDictionary { get; }

        public int Stack { get; }

        private bool IsStackOk(CharacterStatus characterStatus)
        {
            if (DoneDictionary.TryGetValue(characterStatus, out var nowStack)
            )
            {
                return nowStack <= Stack;
            }

            DoneDictionary[characterStatus] = 0;
            return true;
        }

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