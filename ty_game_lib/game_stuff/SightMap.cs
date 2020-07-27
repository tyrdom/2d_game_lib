using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class SightMap
    {
        public readonly IQSpace Lines;

        public SightMap(IQSpace lines)
        {
            Lines = lines ?? throw new ArgumentNullException(nameof(lines));
        }

        public static void GenByConfig(List<(Poly, bool)> lp, TwoDVectorLine[] lines) //todo
        {
            var twoDVectorLines = lp.SelectMany(x => x.Item1.CovToLines(x.Item2));
            twoDVectorLines.ToList().AddRange(lines);
        }

        public bool IsBlockSightLine(TwoDVectorLine s)
        {
            var isTouchBy = Lines.LineIsBlockSight(s);
            return isTouchBy;
        }
    }
}