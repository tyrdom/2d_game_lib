using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class PlayingItemBag
    {
        private Dictionary<int, uint> GameItems { get; }

        public PlayingItemBag(Dictionary<int, uint> gameItems)
        {
            GameItems = gameItems;
        }

        public static PlayingItemBag GenByConfig()
        {
            var dictionary = TempConfig.Configs.items.Values.Where(x => x.IsPlayingItem)
                .ToDictionary(x => x.id, _ => (uint) 0);
            return new PlayingItemBag(dictionary);
        }

        public bool CanCost(ISaleUnit saleUnit)
        {
            var canCost = CanCost(saleUnit.Cost);
            return canCost || saleUnit.OrCosts.Any(CanCost);
        }

        private bool CanCost(GameItem gameItem)
        {
            var gameItemItemId = gameItem.ItemId;
            if (!GameItems.TryGetValue(gameItemItemId, out var num)) return false;
            var cost = gameItem.Num;
            return cost <= num;
        }

        public bool Cost(ISaleUnit saleUnit)
        {
            var cost = Cost(saleUnit.Cost);
            return cost || saleUnit.OrCosts.Any(Cost);
        }

        private bool Cost(GameItem gameItem)
        {
            var gameItemItemId = gameItem.ItemId;
            if (!GameItems.TryGetValue(gameItemItemId, out var num)) return false;
            var cost = gameItem.Num;
            if (cost > num) return false;
            GameItems[gameItemItemId] = num - cost;
            return true;
        }

        public void Gain(GameItem gameItem)
        {
            var gameItemItemId = gameItem.ItemId;
            if (!GameItems.TryGetValue(gameItemItemId, out var num)) return;
            var min = Math.Min(GetMaxNum(gameItemItemId), num + gameItem.Num);
            GameItems[gameItemItemId] = min;
        }

        private static uint GetMaxNum(int itemId)
        {
            if (TempConfig.Configs.items.TryGetValue(itemId, out var item))
            {
                return item.MaxStack;
            }

            throw new Exception($"not such  item id::{itemId}");
        }
    }

    public readonly struct GameItem
    {
        public GameItem(int itemId, uint num)
        {
            ItemId = itemId;
            Num = num;
        }

        public int ItemId { get; }
        public uint Num { get; }


        uint GetMaxNum()
        {
            if (TempConfig.Configs.items.TryGetValue(ItemId, out var item))
            {
                return item.MaxStack;
            }

            throw new Exception($"not such  item id::{ItemId}");
        }
    }


    public readonly struct KillBox
    {
        public KillBox(List<GameItem> mustDropItems,
            (int sum, List<(int wegihtNum, IMapInteractable mapInteractable)> MapInteractables)
                weightMapInteractableDrop)
        {
            MustDropItems = mustDropItems;
            WeightMapInteractableDrop = weightMapInteractableDrop;
        }


        private List<GameItem> MustDropItems { get; }

        private (int sum, List<(int wegihtNum, IMapInteractable mapInteractable)> MapInteractables)
            WeightMapInteractableDrop { get; }

        private IMapInteractable? DropMapInteractable(Random random, TwoDPoint pos)
        {
            var (sum, mapInteractables) = WeightMapInteractableDrop;
            var next = random.Next(sum);
            var firstOrDefault = mapInteractables.FirstOrDefault(
                    x => next < x.wegihtNum)
                .mapInteractable;
            firstOrDefault?.ReLocate(pos);
            return firstOrDefault;
        }

        void GainItemToChars(List<CharacterStatus> characterStatuses)
        {
        }
    }
}