using System.Collections.Generic;
using collision_and_rigid;

namespace game_bot
{
    public class PatrolCtrl
    {
        public TwoDPoint[] Points { get; }
        public int NowToPt { get; set; }

        public PatrolCtrl(List<TwoDPoint> rawPoints)
        {
            var twoDPoints = rawPoints.GetRange(1, rawPoints.Count - 2);
            twoDPoints.Reverse();
            rawPoints.AddRange(twoDPoints);
            Points = rawPoints.ToArray();
            NowToPt = 0;
        }

        public TwoDPoint GetNowPt()
        {
            return Points[NowToPt];
        }

        public TwoDPoint NextPt()
        {
            NowToPt = (1 + NowToPt) % Points.Length;
            return Points[NowToPt];
        }
    }
}