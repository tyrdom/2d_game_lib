using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public ISaleStuff? GetTitleGood();
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
            return IsStackOk(characterStatus, saleUnit) && characterStatus.PlayingItemBag.CanCost(saleUnit);
        }

        public static bool CanInterActTwoBy(CharacterStatus characterStatus, ISaleUnit saleUnit)
        {
            return IsStackOk(characterStatus, saleUnit) && characterStatus.PlayingItemBag.CanCost(saleUnit);
        }

        public static ImmutableArray<IActResult> ActWhichChar(CharacterStatus characterStatus,
            MapInteract interactive, ISaleUnit saleUnit
        )
        {
            switch (interactive)
            {
                case MapInteract.GetInfoCall:
                    if (GenActResultWhichChar(characterStatus, saleUnit, out var c1)) return c1;
                    break;
                case MapInteract.BuyOrApplyCall:
                    if (GenActResultWhichChar(characterStatus, saleUnit, out var c)) return c;
                    break;
                default: return ImmutableArray<IActResult>.Empty;
            }

            return ImmutableArray<IActResult>.Empty;
        }

        private static bool GenActResultWhichChar(CharacterStatus characterStatus, ISaleUnit saleUnit,
            out ImmutableArray<IActResult> c)
        {
            c = ImmutableArray<IActResult>.Empty;
            var cost = characterStatus.PlayingItemBagCost(saleUnit);
            var characterStatusGId = characterStatus.GId;
            var restStack = saleUnit.GetRestStack(characterStatusGId);
            var b = restStack > 0;
            if (!cost || !b) return false;
            if (saleUnit.DoneDictionary.TryGetValue(characterStatusGId, out var haveUsed))
            {
                saleUnit.DoneDictionary[characterStatusGId] = haveUsed + 1;
            }
            else
            {
                saleUnit.DoneDictionary[characterStatusGId] = 1;
            }


            var saleStuffs = saleUnit.GetGood();

            var selectMany = saleStuffs.SelectMany(x =>
            {
                switch (x)
                {
                    case PassiveTrait passiveTrait:
                        var immutableArray =
                            passiveTrait.ActWhichChar(characterStatus, MapInteract.PickCall);
                        return immutableArray;
                    case Prop prop:
                        var actResults = prop.ActWhichChar(characterStatus, MapInteract.PickCall);
                        return actResults;
                    case Vehicle vehicle:
                        vehicle.WhoDriveOrCanDrive = characterStatus;
                        var twoDPoint = saleUnit.InWhichMapInteractive?.GetAnchor() ??
                                        characterStatus.GetPos();

                        var dropAsIMapInteractable = vehicle.DropAsIMapInteractable(twoDPoint);
                        var actWhichChar = new DropThings(new List<IMapInteractable>
                            {dropAsIMapInteractable});
                        var results = new IActResult[] {actWhichChar};
                        return results.ToImmutableArray();
                    case Weapon weapon:
                        return weapon.ActWhichChar(characterStatus, MapInteract.PickCall);
                    default:
                        return ImmutableArray<IActResult>.Empty;
                }
            });
            var mapMarkId = saleUnit.InWhichMapInteractive?.MapMarkId;
            if (!mapMarkId.HasValue)
            {
                {
                    c = selectMany.ToImmutableArray();
                    return true;
                }
            }

            var saleStackChange = new SaleStackChange(mapMarkId.Value, restStack - 1);
            var enumerable = selectMany.Append(saleStackChange).ToImmutableArray();
            {
                c = enumerable;
                return true;
            }
        }

        public static ISaleUnit GenById(int i)
        {
            if (CommonConfig.Configs.sale_units.TryGetValue(i, out var saleUnit))
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

    public readonly struct SaleStackChange : IToOutPutResult
    {
        public SaleStackChange(int mapMarkId, int restStack)
        {
            MapMarkId = mapMarkId;
            RestStack = restStack;
        }

        public int RestStack { get; }

        public int MapMarkId { get; }
    }
}