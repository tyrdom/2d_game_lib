using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_bot
{
    public class PatrolCtrl
    {
        private TwoDPoint[] Points { get; }
        private int NowToPt { get; set; }

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


        public int GetPtNum()
        {
            return Points.Length;
        }

        public TwoDPoint NextPt()
        {
            NowToPt = (1 + NowToPt) % Points.Length;
            return Points[NowToPt];
        }

        public TwoDPoint[] NextPt(int num)
        {
            var start = NowToPt;

            var end = NowToPt + num;

            var pointsLength = (start + end) / Points.Length;
            var twoDPoints = Points.ToList();
            for (var i = 0; i < pointsLength; i++)
            {
                twoDPoints.AddRange(Points);
            }
#if DEBUG
            Console.Out.WriteLine($"get patrol num {twoDPoints.Count}");
#endif
            var dPoints = twoDPoints.Skip(start).Take(num);
            NowToPt += num;
            return dPoints.ToArray();
        }
    }
}