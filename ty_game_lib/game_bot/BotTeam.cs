using System.Collections.Generic;
using System.Linq;
using cov_path_navi;
using game_config;
using game_stuff;

namespace game_bot
{
    public class BotTeam
    {
        private Dictionary<size, PathTop> SizeToNaviMap { get; set; }
        private Dictionary<int, SimpleBot> SimpleBots { get; }

        public BotTeam()
        {
            SimpleBots = new Dictionary<int, SimpleBot>();
            SizeToNaviMap = new Dictionary<size, PathTop>();
        }

        public void SetNaviMaps(WalkMap walkMap)
        {
            SizeToNaviMap = walkMap.SizeToEdge.ToDictionary(p => p.Key,
                p => new PathTop(p.Value));
        }

        public void SetBot(SimpleBot simpleBot)
        {
            var id = simpleBot.BotBody.GetId();

            SimpleBots[id] = simpleBot;
        }
    }
}