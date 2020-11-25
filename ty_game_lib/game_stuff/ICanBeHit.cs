using collision_and_rigid;

namespace game_stuff
{
    public interface ICanBeHit : IIdPointShape
    {
        BodySize GetSize();
        bool CheckCanBeHit();
    }
}