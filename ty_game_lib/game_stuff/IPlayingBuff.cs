using game_config;

namespace game_stuff
{
    public interface IPlayingBuff
    {
        uint RestTick { get; set; }
        bool IsFinish();

        bool AddToCharacter();
    }


    public class AttackBuff : IPlayingBuff
    {
        public uint RestTick { get; set; }

        public bool IsFinish()
        {
            throw new System.NotImplementedException();
        }

        public bool AddToCharacter()
        {
            throw new System.NotImplementedException();
        }
    }

    public static class PlayBuffStandard
    {
        public static bool IsFinish(IPlayingBuff playingBuff)
        {
            return playingBuff.RestTick <= 0;
        }
    }
}