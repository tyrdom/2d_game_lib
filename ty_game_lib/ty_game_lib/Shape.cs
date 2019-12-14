using System;

namespace ty_game_lib
{
    public interface Shape
    {
        AabbBox CovToAabbBox();
        bool IsTouch(Round another);
    }


  
}