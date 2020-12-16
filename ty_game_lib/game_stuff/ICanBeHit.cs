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


    public interface ICanBeSaw : IHaveAnchor
    {
        ISeeTickMsg GenTickMsg(int? gid = null);
    }

    public interface ISeeTickMsg
    {
        string ToString();
    }

    public static class CanBeHitStandard
    {
    }
}