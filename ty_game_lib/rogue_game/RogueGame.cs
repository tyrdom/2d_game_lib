using System;
using System.Collections.Generic;
using game_stuff;

namespace rogue_game
{
    public class RogueGame
    {
        
        private Dictionary<int, PlayGround> Maps { get; }

        public PveRogueRuler PveRogueRuler { get; }

        public RogueGame(Dictionary<int, PlayGround> maps, PveRogueRuler pveRogueRuler)
        {
            Maps = maps;
            PveRogueRuler = pveRogueRuler;
        }
        
        
    }
}