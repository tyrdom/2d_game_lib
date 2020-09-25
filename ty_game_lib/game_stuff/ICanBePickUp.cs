using collision_and_rigid;

namespace game_stuff
{
    public interface IMapInteractive  : IAaBbBox
    {
        Round CanInterActiveRound { get; }
    }

    public interface ICanPutInCage
    {
    }
}