using System;
using System.Collections.Generic;

namespace collision_and_rigid
{
    public class SimpleBlocks : IBulletShape
    {
        private List<AabbBoxShape> AabbBoxShapes;

        public SimpleBlocks(List<AabbBoxShape> aabbBoxShapes)
        {
            AabbBoxShapes = aabbBoxShapes;
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
    }
}