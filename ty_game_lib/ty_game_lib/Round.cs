using System.Collections.Generic;

namespace ty_game_lib
{
    public class Round : IShape
    {
        public TwoDPoint O;
        public float R;

        public Round(TwoDPoint o, float r)
        {
            O = o;
            R = r;
        }

        public AabbBoxShape CovToAabbPackBox()
        {
            var zone = new Zone(O.Y + R, O.Y - R, O.X - R, O.X + R);
            return new AabbBoxShape(zone, this);
        }

        public int TouchByRightShootPointInAAbbBox(TwoDPoint p)
        {
            throw new System.NotImplementedException();
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