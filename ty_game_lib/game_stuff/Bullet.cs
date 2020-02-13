using collision_and_rigid;

namespace game_stuff
{
    public class Bullet
    {
        private TwoDPoint Anchor;

        private AabbBoxShape coverUseShape;
        private CharacterStatus master;

        private TwoDVector PushSpeed;

        private PushType PushType;

        private Zone raw;

        private float upPower;
    }

    internal enum PushType
    {
        Vector,
        FromAnchor,
        Grip
    }
}