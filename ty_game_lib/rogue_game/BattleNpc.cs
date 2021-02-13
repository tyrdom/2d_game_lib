using System;
using game_stuff;

namespace rogue_game
{
    public class BattleNpc
    {
        public BattleNpc(CharacterBody characterBody, WantedBonus wantedBonus)
        {
            CharacterBody = characterBody;
            WantedBonus = wantedBonus;
        }

        public CharacterBody CharacterBody { get; }
        public WantedBonus WantedBonus { get; }

        public static BattleNpc GenByConfig(game_config.battle_npc battleNpc)
        {
            throw new NotImplementedException();
        }
    }
}