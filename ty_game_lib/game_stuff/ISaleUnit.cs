using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface IApplyUnit : ICanPutInMapInteractable
    {
    }

    public interface ISaleUnit : IApplyUnit
    {
        GameItem Cost { get; }

        GameItem[] OrCosts { get; }
        Dictionary<int, int> DoneDictionary { get; }

        int Stack { get; }

        ISaleStuff[] GetGood();

        int GetRestStack(int? gid);
    
    }


    public static class SaleUnitStandard
    {
        public static int GetRestStack(int? gid, ISaleUnit saleUnit)
        {
            if (gid != null && saleUnit.DoneDictionary.TryGetValue(gid.Value, out var haveUsed))
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
            return IsStackOk(characterStatus, saleUnit) && characterStatus.PlayingItemBag.CanCost(saleUnit);
        }

        public static IActResult? ActWhichChar(CharacterStatus characterStatus,
            MapInteract interactive, ISaleUnit saleUnit
        )
        {
            switch (interactive)
            {
                case MapInteract.GetInfoCall:
                    return null;
                case MapInteract.BuyOrApplyCall:
                    var cost = characterStatus.PlayingItemBag.Cost(saleUnit);
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

                        var saleStuffs = saleUnit.GetGood();
                        foreach (var saleStuff in saleStuffs)
                        {
                            switch (saleStuff)
                            {
                                case PassiveTrait passiveTrait:
                                    return passiveTrait.ActWhichChar(characterStatus, MapInteract.PickCall);
                                case Prop prop:
                                    return prop.ActWhichChar(characterStatus, MapInteract.PickCall);
                                case Vehicle vehicle:
                                    vehicle.WhoDriveOrCanDrive = characterStatus;
                                    var twoDPoint = saleUnit.InWhichMapInteractive?.GetAnchor() ??
                                                    characterStatus.GetPos();

                                    var dropAsIMapInteractable = vehicle.DropAsIMapInteractable(twoDPoint);
                                    return new DropThings(new List<IMapInteractable> {dropAsIMapInteractable});
                                case Weapon weapon:
                                    return weapon.ActWhichChar(characterStatus, MapInteract.PickCall);
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(saleUnit.GetGood));
                            }
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(interactive), interactive, null);
            }

            return null;
        }

        public static ISaleUnit GenById(int i)
        {
            
            if(CommonConfig.Configs.sale_units.TryGetValue(i,out var saleUnit))
            {
                return GenByConfig(saleUnit);
            }

            throw new DirectoryNotFoundException($"not such id {i}");
        }

        private static ISaleUnit GenByConfig(sale_unit saleUnit)
        {
            return saleUnit.IsRandomSale switch
            {
                true => SaleRandom.GenByConfig(saleUnit),
                false => SaleUnit.GenByConfig(saleUnit)
            };
        }
    }
}