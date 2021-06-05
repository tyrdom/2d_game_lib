using collision_and_rigid;

namespace game_stuff
{
    public interface ICanBeAndNeedSaw : IPerceivable
    {
        ISeeTickMsg GenTickMsg(int? gid = null);
    }

    public interface IPerceivable : IHaveAnchor
    {
    }

    public interface IMapMarkId
    {
        int MapMarkId { get; set; }
    }

    public interface INotMoveCanBeAndNeedSew : ICanBeAndNeedSaw, IMapMarkId
    {
    }

    public interface ICanBeEnemy : IPerceivable
    {
    }
}