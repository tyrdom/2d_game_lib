using System;
using System.Collections;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public interface ISaleUnit : ICanPutInMapInteractable
    {
        GameItem Cost { get; }
        Dictionary<int, int> DoneDictionary { get; }

        int Stack { get; }

        ISaleStuff GetGood();

        int GetRestStack(int gid);
    }


    public static class SaleUnitStandard
    {
        public static int GetRestStack(int gid, ISaleUnit saleUnit)
        {
            if (saleUnit.DoneDictionary.TryGetValue(gid, out var haveUsed))
            {
                return saleUnit.Stack - haveUsed;
            }

            return saleUnit.Stack;
        }

        private static bool IsStackOk(CharacterStatus characterStatus, ISaleUnit saleUnit)
        {
            if (saleUnit.DoneDictionary.TryGetValue(characterStatus.GId, out var nowStack)
            )
            {
                return nowStack <= saleUnit.Stack;
            }

            saleUnit.DoneDictionary[characterStatus.GId] = 0;
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
                    return new List<IMapInteractable>();
                case MapInteract.BuyCall:
                    var cost = characterStatus.PlayingItemBag.Cost(saleUnit.Cost);
                    var characterStatusGId = characterStatus.GId;
                    var b = saleUnit.GetRestStack(characterStatusGId) > 0;
                    if (cost && b)
                    {
                        if (saleUnit.DoneDictionary.TryGetValue(characterStatusGId, out var haveUsed))
                        {
                            saleUnit.DoneDictionary[characterStatusGId] = haveUsed + 1;
                        }
                        else
                        {
                            saleUnit.DoneDictionary[characterStatusGId] = 1;
                        }

                        switch (saleUnit.GetGood())
                        {
                            case PassiveTrait passiveTrait:
                                return passiveTrait.ActWhichChar(characterStatus, MapInteract.PickCall);
                            case Prop prop:
                                return prop.ActWhichChar(characterStatus, MapInteract.PickCall);
                            case Vehicle vehicle:
                                vehicle.WhoDriveOrCanDrive = characterStatus;
                                var twoDPoint = saleUnit.InWhichMapInteractive?.GetAnchor() ?? characterStatus.GetPos();

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