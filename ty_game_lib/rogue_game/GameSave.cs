using System;
using System.Collections.Generic;
using System.Net.Http;
using game_stuff;

namespace rogue_game
{
    public class GameSave
    {
        private Dictionary<int, PlayerSave> PlayerSaves { get; }

        private MapSave MapSave { get; }

        private int ChapterId { get; }
    }
}