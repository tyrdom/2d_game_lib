using collision_and_rigid;

namespace game_stuff
{
    public class BulletMsg
    {
        private TwoDPoint Pos;
        private TwoDVector Aim;
        private int ResId;
        private TwoDPoint? MastPos;

        public BulletMsg(TwoDPoint pos, TwoDVector aim, int resId, TwoDPoint? mastPos)
        {
            Pos = pos;
            Aim = aim;
            ResId = resId;
            MastPos = mastPos;
        }
    }
}