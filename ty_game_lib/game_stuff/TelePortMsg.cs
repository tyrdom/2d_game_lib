using collision_and_rigid;

namespace game_stuff
{
    public readonly struct TelePortMsg : IActResult
    {
        public TelePortMsg(int mgId, TwoDPoint toPos)
        {
            GMid = mgId;
            ToPos = toPos;
        }

        public int GMid { get; }
        public TwoDPoint ToPos { get; }

        public void Deconstruct(out int gMid, out TwoDPoint toPos)
        {
            gMid = GMid;
            toPos = ToPos;
        }
    }
}