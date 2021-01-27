using collision_and_rigid;

namespace game_stuff
{
    public interface ICanBeHit : IIdPointShape, ICanBeSaw
    {
        IdPointBox InBox { get; set; }
        BodySize GetSize();
        bool CheckCanBeHit();
        public IdPointBox CovToIdBox();
    }


    public interface ISeeTickMsg
    {
        string ToString();
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