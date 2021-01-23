using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class StartPts
    {
        private int Now { get; set; }
        private TwoDPoint[] Pts { get; }

        public StartPts(TwoDPoint[] pts)
        {
            Pts = pts;
            Now = 0;
        }

        public IEnumerable<TwoDPoint> GenPt(int a)
        {
            var ptsLength = Pts.Length;
            var enumerable = Enumerable.Range(Now, a).Select(x => Pts[x % ptsLength]);
            Now = (Now + a) % ptsLength;

            return enumerable.ToArray();
        }

        public TwoDPoint GenPt()
        {
            var ptsLength = Pts.Length;
            var twoDPoint = Pts[Now % ptsLength];
            Now = (Now + 1) % ptsLength;
            return twoDPoint;
        }
    }
}