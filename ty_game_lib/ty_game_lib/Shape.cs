using System;

namespace ty_game_lib
{
    public interface Shape
    {
        AabbBoxShape CovToAabbPackBox();

        int
            TouchByRightShootPointInAAbbBox(TwoDPoint p);
    }
}