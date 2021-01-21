using game_stuff;

namespace rogue_game
{
    public class RogueGamePlayer
    {
        public RogueGamePlayer(CharacterBody player)
        {
            Player = player;
            FinalBill = new FinalDeal();
            IsDead = false;
        }

        public bool IsDead { get; set; }

        public CharacterBody Player { get; }


        public FinalDeal FinalBill { get; }

        public void Teleport(PveMap pveMap)
        {
        }

        public PlayerStatusSave PlayerSave(int mid)
        {
            var playerCharacterStatus = Player.CharacterStatus;
            var playerStatusSave = PlayerStatusSave.GenPlayerSave(playerCharacterStatus, mid);
            return playerStatusSave;
        }

        public void CheckDead()
        {
            IsDead = Player.CharacterStatus.SurvivalStatus.IsDead();
        }
    }
}