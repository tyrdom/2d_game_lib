using game_config;

namespace game_stuff
{
    public interface IPlayBuff
    {
        uint RestTick { get; set; }
        bool IsFinish();

        bool AddToCharacter();
    }

    public static class PlayBuffStandard
    {
    }
}