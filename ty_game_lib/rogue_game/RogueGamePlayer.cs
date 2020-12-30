using game_stuff;

namespace rogue_game
{
    public class RogueGamePlayer
    {
        public RogueGamePlayer(CharacterBody player, PveMap inPlayPveMap, int lastTravelId, PlayerSave playerSave)
        {
            Player = player;
            InPlayGround = inPlayPveMap;
            LastTravelId = lastTravelId;
            PlayerSave = playerSave;
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
            var lastTravelId = InPlayGround.IsClear();
            if (lastTravelId)
            {
                var mId = InPlayGround.GetMId();
                LastTravelId = mId;
            }

            InPlayGround = pveMap;
        }
    }
}