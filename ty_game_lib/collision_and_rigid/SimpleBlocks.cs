using System;
using System.Collections.Generic;
using System.Linq;

namespace collision_and_rigid
{
    public class SimpleBlocks : IBulletShape
    {
        private List<AabbBoxShape> AabbBoxShapes;

        public SimpleBlocks(List<AabbBoxShape> aabbBoxShapes)
        {
            AabbBoxShapes = aabbBoxShapes;
        }

        public SimpleBlocks(IEnumerable<IBlockShape> blockShapes)
        {
            AabbBoxShapes = blockShapes.SelectMany(x => x.GenAabbBoxShape()).ToList();
        }

        public bool IsEmpty()
        {
            return AabbBoxShapes.Count <= 0;
        }

        public HashSet<IBlockShape> GetBlockShapes()
        {
            var blockShapes = AabbBoxShapes.Select(x => x.Shape).OfType<IBlockShape>().ToList();
            var listToHashSet = SomeTools.ListToHashSet(blockShapes);
            return listToHashSet;
        }

        public bool PtInShape(TwoDPoint point)
        {
            var (item1, _) = point.GenARightShootCrossALotAabbBoxShape(AabbBoxShapes);
            if (item1 >= 0)
            {
                return item1 % 2 != 0;
            }

            return item1 == -1;
        }

        public bool LineCross(TwoDVectorLine line)
        {
            foreach (var aabbBoxShape in AabbBoxShapes)
            {
                bool notCross = line.GenZone().NotCross(aabbBoxShape.Zone);
                if (!notCross)
                {
#if DEBUG
                    Console.Out.WriteLine($"line { line.Log()} is cross zone ::{aabbBoxShape.Zone.LogSide()}");
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
            }

            return false;
        }
    }
}