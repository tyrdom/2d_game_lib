using System.Collections.Generic;
using System.Numerics;

namespace ty_game_lib
{
    public class TwoDPoint
    {
        public TwoDPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public readonly float X;
        public readonly float Y;

        public (int, AabbBoxShape?) GenARightShootCrossAlotAabbBoxShape(Zone azone, List<AabbBoxShape> aabbBoxShapes)
        {
            var n = 0;
            AabbBoxShape? aShape = null;

            foreach (var aabbBoxShape in aabbBoxShapes)
            {
                var zone = aabbBoxShape.Zone;
                if (Y <= zone.Up && Y > zone.Down
                )
                {
                    if (X <= azone.Left)
                    {
                        n++;
                    }
                    else if (X > zone.Left && X < zone.Right)
                    {
                        aShape = aabbBoxShape;
                        var touchByRightShootPointInAAbbBox = aabbBoxShape._shape.TouchByRightShootPointInAAbbBox(this);
                        n = n + touchByRightShootPointInAAbbBox;
                    }
                }
            }

            return (n, aShape);
        }

        public (TwoDPoint, TwoDPoint) SwapPoint(TwoDPoint b)
        {
            return (b, this);
        }

        public float Get2S(TwoDVectorLine aline)
        {
            var xLine = new TwoDVectorLine(aline.A, this);

            return xLine.GetVector().Cross(aline.GetVector());
        }

        public Pt2LinePos GetposOnLine(TwoDVectorLine aline)
        {
            var cross = Get2S(aline);
            return cross switch
            {
                { } cro when cro > 0 => Pt2LinePos.Right,
                { } cro when cro < 0 => Pt2LinePos.Left,
                _ => Pt2LinePos.On
            };
        }

        public TwoDPoint move(TwoDVector v)
        {
            return new TwoDPoint(X + v.X, Y + v.Y);
        }
    }
}