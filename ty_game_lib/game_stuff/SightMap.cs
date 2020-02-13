using System;
using collision_and_rigid;

namespace game_stuff
{
    public class SightMap
    {
        private readonly QSpace Lines;

        public SightMap(QSpace lines)
        {
            Lines = lines ?? throw new ArgumentNullException(nameof(lines));
        }


        private bool IsBlockSightLine(TwoDVectorLine s)
        {
            var isTouchBy = Lines.IsTouchBy(s.CovToAabbPackBox());
            return isTouchBy;
        }
    }
}