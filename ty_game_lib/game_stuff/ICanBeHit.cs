using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface ICanBeHit : IIdPointShape, ICanBeSaw, ICanBeEnemy
    {
        int GetTeam();
        IdPointBox InBox { get; set; }
        size GetSize();
        bool CheckCanBeHit();
        public IdPointBox CovToIdBox();
    }


    public interface ISeeTickMsg
    {
        string? ToString();
    }

    public interface ISeeActive
    {
        int AId { get; }
        bool IsActive { get; }
    }

    public interface ISeePosChange
    {
        TwoDPoint Pos { get; }
    }

    public static class CanBeHitStandard
    {
    }
}