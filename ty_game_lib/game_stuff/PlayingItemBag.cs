using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class PlayingItemBag
    {
        public Dictionary<int, int> GameItems { get; }

        public PlayingItemBag(Dictionary<int, int> gameItems)
        {
            GameItems = gameItems;
        }

        public static PlayingItemBag InitByConfig()
        {
            var dictionary = LocalConfig.Configs.items.Values.Where(x => x.IsPlayingItem)
                .ToDictionary(x => x.id, _ => 0);
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

        public bool CanCost(IEnumerable<GameItem> gameItem)
        {
            var gameItems = GameItem.SumSame(gameItem);
            return gameItems.All(CanCost);
        }

        public bool Cost(IEnumerable<GameItem> gameItems)
        {
            var gameItem = gameItems as GameItem[] ?? gameItems.ToArray();
            if (!CanCost(gameItem)) return false;
            var all = gameItem.All(Cost);
            return all;
        }

        public bool Cost(GameItem gameItem)
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
            GameItems[gameItemItemId] = (int) min;
        }

        private static uint GetMaxNum(int itemId)
        {
            if (LocalConfig.Configs.items.TryGetValue(itemId, out var item))
            {
                return item.MaxStack;
            }

            throw new Exception($"not such  item id::{itemId}");
        }
    }


    public readonly struct GameItem : ISaleStuff
    {
        public static IEnumerable<GameItem> SumSame(IEnumerable<GameItem> gameItems)
        {
            var gameItem = gameItems.GroupBy(a => a.ItemId).Select(x => new GameItem(x.Key, x.Sum(item => item.Num)));
            return gameItem;
        }

        public static GameItem GenByConfigGain(Gain gain)
        {
            return new GameItem(gain.item
                , gain.num);
        }

        public GameItem(int itemId, int num)
        {
            ItemId = itemId;
            Num = num;
        }

        public int ItemId { get; }
        public int Num { get; }


        private uint GetMaxNum()
        {
            if (LocalConfig.Configs.items.TryGetValue(ItemId, out var item))
            {
                return item.MaxStack;
            }

            throw new Exception($"not such  item id::{ItemId}");
        }

        public int GetId()
        {
            return ItemId;
        }

        public int GetNum()
        {
            return Num;
        }
    }


    public interface IItem
    {
    }


    public readonly struct KillDrops
    {
        public KillDrops(List<GameItem> mustDropItems,
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