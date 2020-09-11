using System.Collections.Generic;

namespace game_bot
{
    public class BotTeam
    {
        private Dictionary<int, Bot> Bots;

        public BotTeam(Dictionary<int, Bot> bots)
        {
            Bots = bots;
        }
    }
}