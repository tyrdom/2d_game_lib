using collision_and_rigid;

namespace game_stuff
{
    public interface ICanBeSaw : IPerceivable
    {
        ISeeTickMsg GenTickMsg(int? gid = null);
    }

    public interface IPerceivable : IHaveAnchor
    {
    }

    public interface INotMoveCanBeSew : ICanBeSaw
    {
        int MapInstanceId { get; set; }
    }

    public interface ICanBeEnemy : IPerceivable
    {
    }
}