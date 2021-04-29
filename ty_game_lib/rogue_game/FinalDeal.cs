using System.Collections.Generic;
using System.Linq;
using game_config;
using game_stuff;

namespace rogue_game
{
    public class FinalDeal
    {
        public IDictionary<item_id, int> Cost { get; }
        public IDictionary<item_id, int> Gain { get; }

        public FinalDeal()
        {
            Cost = new Dictionary<item_id, int>();
            Gain = new Dictionary<item_id, int>();
        }

        private static void Add(IEnumerable<GameItem> gameItems, IDictionary<item_id, int> add)
        {
            {
                var sumSame = GameItem.SumSame(gameItems);
                var enumerable = sumSame.Where(x => RogueLocalConfig.CanEndRest(x.ItemId));
                foreach (var gameItem in enumerable)
                {
                    var gameItemItemId = gameItem.ItemId;
                    var itemNum = gameItem.Num;
                    if (add.TryGetValue(gameItemItemId, out var num))
                    {
                        var gameItemNum = num + itemNum;
                        add[gameItemItemId] = gameItemNum;
                    }
                    else
                    {
                        add[gameItemItemId] = itemNum;
                    }
                }
            }
        }

        public void AddCost(IEnumerable<GameItem> gameItems)
        {
            Add(gameItems, Cost);
        }

        public void AddGain(IEnumerable<GameItem> gameItems)
        {
            Add(gameItems, Gain);
        }
    }
}