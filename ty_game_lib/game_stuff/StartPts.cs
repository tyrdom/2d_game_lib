using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class StartPts
    {
        private int Now { get; set; }
        private List<TwoDPoint> Pts { get; }

        public StartPts(List<TwoDPoint> pts)
        {
            Pts = pts;
            Now = 0;
        }

        public TwoDPoint GenPt()
        {
            var twoDPoint = Pts[Now % Pts.Count];
            Now += 1;
            return twoDPoint;
        }
    }
}