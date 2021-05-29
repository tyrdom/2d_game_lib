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
            if (rawPoints.Count < 2)
            {
                Points = new TwoDPoint[] { };
                NowToPt = 0;
                return;
            }

            var twoDPoints = rawPoints.GetRange(1, rawPoints.Count - 2);
            twoDPoints.Reverse();
            rawPoints.AddRange(twoDPoints);
            Points = rawPoints.ToArray();
        }

        public TwoDPoint GetNowPt()
        {
            return Points[NowToPt];
        }


        public int GetPtNum()
        {
            return Points.Length;
        }


        public IEnumerable<TwoDPoint> NextPt(int num)
        {
            if (!Points.Any())
            {
                return Points;
            }

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
            NowToPt = end % Points.Length;
            return dPoints.ToArray();
        }
    }
}