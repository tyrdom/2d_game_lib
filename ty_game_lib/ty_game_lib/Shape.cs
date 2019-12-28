using System;

namespace ty_game_lib
{
    public interface IShape
    {
        AabbBoxShape CovToAabbPackBox();

        int TouchByRightShootPointInAAbbBox(TwoDPoint p);
        
        
    }


    
}