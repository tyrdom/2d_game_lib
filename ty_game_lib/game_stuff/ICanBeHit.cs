using collision_and_rigid;

namespace game_stuff
{
    public interface ICanBeHit : IIdPointShape, ICanBeSaw
    {
        BodySize GetSize();
        bool CheckCanBeHit();
        public IdPointBox CovToIdBox();
    }


    public interface ICanBeSaw : IHaveAnchor
    {
        ISeeTickMsg GenTickMsg();
    }

    public interface ISeeTickMsg
    {
    }

    public static class CanBeHitStandard
    {
    }
}