using collision_and_rigid;

namespace game_stuff
{
    public interface ICanBeSaw : IHaveAnchor, IPerceivable
    {
        ISeeTickMsg GenTickMsg(int? gid = null);
    }

    public interface IPerceivable
    {
    }
}