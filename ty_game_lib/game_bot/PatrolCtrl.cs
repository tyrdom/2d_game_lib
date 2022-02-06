using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_bot
{
    public class PatrolCtrl
    {
        public TwoDPoint[] Points { get; }
       

        public PatrolCtrl(List<TwoDPoint> rawPoints)
        {
            if (rawPoints.Count < 2)
            {
                Points = new TwoDPoint[] { };
                
                return;
            }

            var twoDPoints = rawPoints.GetRange(1, rawPoints.Count - 2);
            twoDPoints.Reverse();
            rawPoints.AddRange(twoDPoints);
            Points = rawPoints.ToArray();
        }


        public int GetPtNum()
        {
            return Points.Length;
        }

        public TwoDPoint GetPt(int i)
        {
            return Points[i];
        }

        public IEnumerable<TwoDPoint> TakePt(int start, int num)
        {
            if (!Points.Any())
            {
                return Points;
            }


            var end = start + num;

            var pointsLength = end / Points.Length;
            var twoDPoints = Points.ToList();
            for (var i = 0; i < pointsLength; i++)
            {
                twoDPoints.AddRange(Points);
            }
#if DEBUG
            Console.Out.WriteLine($"get patrol num {twoDPoints.Count}");
#endif
            var dPoints = twoDPoints.Skip(start).Take(num);
            return dPoints.ToArray();
        }
    }
}