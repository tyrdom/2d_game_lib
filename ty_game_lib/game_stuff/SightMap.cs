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

        public static SightMap GenByConfig(IEnumerable<(Poly, bool)> lp, IEnumerable<TwoDVectorLine> lines)
        {
            var twoDVectorLines = lp.SelectMany(x => x.Item1.CovToLines(x.Item2));
            twoDVectorLines.ToList().AddRange(lines);
            var aabbBoxShapes = twoDVectorLines.Select(x => x.CovToAabbPackBox());
            var qSpaceByAabbBoxShapes = SomeTools.CreateQSpaceByAabbBoxShapes(aabbBoxShapes.ToArray(), 5);
            var sightMap = new SightMap(qSpaceByAabbBoxShapes);
            return sightMap;
        }

        public bool IsBlockSightLine(TwoDVectorLine s)
        {
            var isTouchBy = Lines.LineIsBlockSight(s);
            return isTouchBy;
        }
    }
}