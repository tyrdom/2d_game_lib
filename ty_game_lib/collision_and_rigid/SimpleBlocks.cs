using System;
using System.Collections.Generic;
using System.Linq;

namespace collision_and_rigid
{
    public class SimpleBlocks : IBulletShape, IShape
    {
        private List<BlockBox> AabbBoxShapes;


        public SimpleBlocks(IEnumerable<IBlockShape> blockShapes)
        {
            var enumerable = blockShapes.ToList();
            var checkFlush = SomeTools.CheckFlush(enumerable);
            if (!checkFlush)
            {
                throw new Exception("not a flush simpleBlocks");
            }

            AabbBoxShapes = enumerable.SelectMany(x => x.GenAabbBoxShape()).ToList();
        }

        public bool IsEmpty()
        {
            return AabbBoxShapes.Count <= 0;
        }

        public HashSet<IBlockShape> GetBlockShapes()
        {
            var blockShapes = AabbBoxShapes.Select(x => x.Shape).ToList();
            var listToHashSet = SomeTools.EnumerableToHashSet(blockShapes);
            return listToHashSet;
        }

        public Zone GenZone()
        {
            var enumerable = AabbBoxShapes.Select(x => x.Zone);
            var zones = enumerable.ToList();
            var first = zones.First();
            return zones.Aggregate(first, (current, zone) => current.Join(zone));
        }

        public bool PtInShapeIncludeSide(TwoDPoint point)
        {
            var (item1, _) = point.GenARightShootCrossALotAabbBoxShapeInQSpace(AabbBoxShapes);
            if (item1 >= 0)
            {
                return item1 % 2 != 0;
            }

            return item1 == -1 || item1 == -3;
        }


        public bool PtRealInShape(TwoDPoint point)
        {
            var (item1, _) = point.GenARightShootCrossALotAabbBoxShapeInQSpace(AabbBoxShapes);
            if (item1 >= 0)
            {
                return item1 % 2 != 0;
            }

            return item1 == -1;
        }

        public bool LineCross(TwoDVectorLine line)
        {
            foreach (var aabbBoxShape in from aabbBoxShape in AabbBoxShapes
                let notCross = line.GenZone().NotCross(aabbBoxShape.Zone)
                where !notCross
                select aabbBoxShape)
            {
#if DEBUG
                // Console.Out.WriteLine($"line {line.Log()} is cross zone ::{aabbBoxShape.Zone.LogSide()}");
#endif
                var b = aabbBoxShape.Shape switch
                {
                    ClockwiseTurning blockClockwiseTurning => blockClockwiseTurning.IsCross(line),
                    TwoDVectorLine blockLine => line.IsGoTrough(blockLine),
                    _ => throw new ArgumentOutOfRangeException()
                };
                if (b)
                {
                    return b;
                }
            }

            return false;
        }

        public (Zone? leftZone, Zone? rightZone) CutByV(float v, Zone z)
        {
            throw new NotImplementedException();
        }

        public (Zone?, Zone?) CutByH(float h, Zone z)
        {
            throw new NotImplementedException();
        }

        public int TouchByRightShootPointInAAbbBoxInQSpace(TwoDPoint p)
        {
            throw new NotImplementedException();
        }
    }
}