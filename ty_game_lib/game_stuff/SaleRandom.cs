﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class SaleRandom : ISaleUnit
    {
        public ContainType Title { get; }

        public static ContainType GetTitle(ISaleStuff saleStuff)
        {
            return saleStuff switch
            {
                PassiveTrait _ => ContainType.PassiveC,
                Prop _ => ContainType.PropC,
                Vehicle _ => ContainType.VehicleC,
                Weapon _ => ContainType.WeaponC,
                _ => throw new ArgumentOutOfRangeException(nameof(saleStuff))
            };
        }

        public SaleRandom(IMapInteractable? inWhichMapInteractive, GameItem cost, int stack,
            ImmutableArray<(int weight, ISaleStuff[] good)> randomGood, Random random, GameItem[] orCosts)
        {
            InWhichMapInteractive = inWhichMapInteractive;
            Cost = cost;
            Stack = stack;
            var (weightOverList, total) = randomGood.GetWeightOverList();
            RandomGood = weightOverList;
            WeightTotal = total;
            Random = random;
            OrCosts = orCosts;
            var firstOrDefault = randomGood.FirstOrDefault().good.FirstOrDefault();
            if (firstOrDefault != null)
            {
                Title = GetTitle(firstOrDefault);
            }

            Title = ContainType.PropC;

            DoneDictionary = new Dictionary<int, int>();
        }

        public int GetRestStack(int? gid)
        {
            return SaleUnitStandard.GetRestStack(gid, this);
        }

        public IMapInteractable PutInteractable(TwoDPoint pos,bool isActive)
        {
            
            return new ApplyDevice(this, pos, isActive);
        }

        public IMapInteractable? InWhichMapInteractive { get; set; }

        public bool CanInterActOneBy(CharacterStatus characterStatus)
        {
            return SaleUnitStandard.CanInterActOneBy(characterStatus, this);
        }

        public bool CanInterActTwoBy(CharacterStatus characterStatus)
        {
            return SaleUnitStandard.CanInterActTwoBy(characterStatus, this);
        }

        public IActResult? ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            return SaleUnitStandard.ActWhichChar(characterStatus, interactive, this);
        }

        public GameItem Cost { get; }
        public GameItem[] OrCosts { get; }
        public Dictionary<int, int> DoneDictionary { get; }
        public int Stack { get; }

        public ISaleStuff[] GetGood()
        {
            var next = Random.Next(WeightTotal);

            var firstOrDefault = RandomGood.GetWeightThings(next).things;
            return firstOrDefault;
        }

        private int WeightTotal { get; }

        private ImmutableArray<(int weightOver, ISaleStuff[] good)> RandomGood { get; }

        private Random Random { get; }
    }
}