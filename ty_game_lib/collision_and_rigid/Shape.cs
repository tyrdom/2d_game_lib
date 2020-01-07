using System;

namespace collision_and_rigid
{
    public interface IShape
    {
        AabbBoxShape CovToAabbPackBox();

        int TouchByRightShootPointInAAbbBox(TwoDPoint p);


        bool IsTouchAnother(IShape another);
    }
}