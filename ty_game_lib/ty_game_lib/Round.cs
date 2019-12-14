using System.Collections.Generic;

namespace ty_game_lib
{
    public class Round : Shape
    {
        public TwoDPoint O;
        public float R;

        public Round(TwoDPoint o, float r)
        {
            O = o;
            R = r;
        }

        public AabbBox CovToAabbBox()
        {
            var zone = new Zone(O.Y + R, O.Y - R, O.X - R, O.X + R);
            return new AabbBox(zone, this);
        }

       

        public bool IsTouch(Round another)
        {
            var rr = another.R + R;
            var oX = O.X - another.O.X;
            var oY = O.Y - another.O.Y;
            return 
                rr * rr <=
                   oX * oX + oY * oY;
        }
    }
}