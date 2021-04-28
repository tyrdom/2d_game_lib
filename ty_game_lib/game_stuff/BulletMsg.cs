using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class BulletMsg
    {
        private TwoDPoint Pos;
        private TwoDVector Aim;
        private bullet_id ResId;
        private TwoDPoint? CastPos;

        public BulletMsg(TwoDPoint pos, TwoDVector aim, bullet_id resId, TwoDPoint? castPos)
        {
            Pos = pos;
            Aim = aim;
            ResId = resId;
            CastPos = castPos;
        }
    }
}