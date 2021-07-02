using System;
using System.Collections.Immutable;
using game_stuff;

namespace rogue_game
{
    [Serializable]
    public class RogueGamePlayer
    {
        public RogueGamePlayer(CharacterBody player)
        {
            Player = player;
            FinalBill = new FinalDeal();
            IsDead = false;
        }

        public bool IsDead { get; private set; }

        public CharacterBody Player { get; }


        public FinalDeal FinalBill { get; }


        public void AddItem(ImmutableList<GameItem> gameItems)
        {
            Player.CharacterStatus.PickGameItem(gameItems);
            FinalBill.AddGain(gameItems);
        }

        public PlayerStatusSave PlayerSave()
        {
            var playerCharacterStatus = Player.CharacterStatus;
            var playerStatusSave = PlayerStatusSave.GenSave(playerCharacterStatus, Player.Team, Player.BodySize);
            return playerStatusSave;
        }

        public void CheckDead()
        {
            IsDead = Player.CharacterStatus.SurvivalStatus.IsDead();
        }
    }
}