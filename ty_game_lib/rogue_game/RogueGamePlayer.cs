using game_stuff;

namespace rogue_game
{
    public class RogueGamePlayer
    {
        public RogueGamePlayer(CharacterBody player, PveMap inPlayPveMap, int lastTravelId)
        {
            Player = player;
            InPlayGround = inPlayPveMap;
            LastTravelId = lastTravelId;
            PlayerSave = PlayerSave.GenPlayerSave(player.CharacterStatus, inPlayPveMap.GetMId());
            FinalBill = new FinalDeal();
        }

        public PlayerSave PlayerSave { get; set; }
        public int LastTravelId { get; set; }
        public CharacterBody Player { get; }
        public PveMap InPlayGround { get; private set; }

        public void SetPveMap(PveMap pveMap)
        {
            InPlayGround = pveMap;
        }

        public FinalDeal FinalBill { get; }

        public void Teleport(PveMap pveMap)
        {
            var lastTravelId = InPlayGround.IsClearAndSave();
            if (lastTravelId)
            {
                var mId = InPlayGround.GetMId();
                LastTravelId = mId;
            }

            InPlayGround = pveMap;
        }

        public void PlayerGoSave()
        {
            var playerCharacterStatus = Player.CharacterStatus;
            PlayerSave.Save(playerCharacterStatus, InPlayGround.GetMId());
        }
    }
}