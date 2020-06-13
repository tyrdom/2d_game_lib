using System;
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


        public bool IsBlockSightLine(TwoDVectorLine s)
        {
            var isTouchBy = Lines.LineIsCross(s);
            return isTouchBy;
        }
    }
}