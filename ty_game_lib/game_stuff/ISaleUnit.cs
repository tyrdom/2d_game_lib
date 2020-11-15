using System;
using System.Collections;
using System.Collections.Generic;

namespace game_stuff
{
    public interface ISaleUnit : ICanPutInMapInteractable
    {
        GameItem Cost { get; }
        Dictionary<CharacterStatus, int> DoneDictionary { get; }

        int Stack { get; }

        ISaleStuff GetGood();
    }

    public static class SaleUnitStandard
    {
        private static bool IsStackOk(CharacterStatus characterStatus, ISaleUnit saleUnit)
        {
            if (saleUnit.DoneDictionary.TryGetValue(characterStatus, out var nowStack)
            )
            {
                return nowStack <= saleUnit.Stack;
            }

            saleUnit.DoneDictionary[characterStatus] = 0;
            return true;
        }

        public static bool CanInterActOneBy(CharacterStatus characterStatus, ISaleUnit saleUnit)
        {
            return IsStackOk(characterStatus, saleUnit);
        }

        public static bool CanInterActTwoBy(CharacterStatus characterStatus, ISaleUnit saleUnit)
        {
            return IsStackOk(characterStatus, saleUnit) && characterStatus.PlayingItemBag.CanCost(saleUnit.Cost);
        }

        public static IEnumerable<IMapInteractable> ActWhichChar(CharacterStatus characterStatus,
            MapInteract interactive, ISaleUnit saleUnit
        )
        {
            switch (interactive)
            {
                case MapInteract.ApplyCall:
                    //todo check msg to char
                    return new List<IMapInteractable>();
                case MapInteract.BuyCall:

                    var cost = characterStatus.PlayingItemBag.Cost(saleUnit.Cost);
                    if (cost)
                    {
                        saleUnit.DoneDictionary[characterStatus] = saleUnit.DoneDictionary[characterStatus] + 1;
                        switch (saleUnit.GetGood())
                        {
                            case PassiveTrait passiveTrait:
                                return passiveTrait.ActWhichChar(characterStatus, MapInteract.PickCall);
                            case Prop prop:
                                return prop.ActWhichChar(characterStatus, MapInteract.PickCall);
                            case Vehicle vehicle:
                                vehicle.WhoDriveOrCanDrive = characterStatus;
                                var twoDPoint = saleUnit.InWhichMapInteractive?.GetPos() ?? characterStatus.GetPos();

                                var dropAsIMapInteractable = vehicle.DropAsIMapInteractable(twoDPoint);
                                return new List<IMapInteractable> {dropAsIMapInteractable};
                            case Weapon weapon:
                                return weapon.ActWhichChar(characterStatus, MapInteract.PickCall);
                            default:
                                throw new ArgumentOutOfRangeException(nameof(saleUnit.GetGood));
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(interactive), interactive, null);
            }

            return new List<IMapInteractable>();
        }
    }
}