using game_config;

namespace game_stuff
{
    public interface IPlayingBuff
    {
        uint RestTick { get; set; }
        bool IsFinish();

        bool AddToCharacter();
    }

    public static class PlayBuffStandard
    {
    }
}