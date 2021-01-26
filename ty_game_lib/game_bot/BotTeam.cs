using System;
using System.Collections.Generic;
using System.Linq;
using cov_path_navi;

namespace game_bot
{
    public class BotTeam
    {
        private Dictionary<int, Bot> Bots { get; }

        public BotTeam(Dictionary<int, Bot> bots)
        {
            Bots = bots;
        }
    }
}