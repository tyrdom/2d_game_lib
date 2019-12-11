namespace ty_game_lib
{
    public class TwoDVectorLine
    {
        public TwoDVectorLine(TwoDPoint a, TwoDPoint b)
        {
            A = a;
            B = b;
        }


        public TwoDPoint A { get; }

        public TwoDPoint B { get; }

        public TwoDVector GetVector()
        {
            return new TwoDVector(B.X - A.X, B.Y - A.Y);
        }

        public TwoDPoint? CrossAnotherPoint(TwoDVectorLine lineB)
        {
            var getposOnLine = A.GetposOnLine(lineB);

            return null;
        }

//        判断线段相交
        public bool IsPointOnAnother(TwoDVectorLine lineB)
        {
            var c = lineB.A;
            var d = lineB.B;
            var getposOnLineA = A.GetposOnLine(lineB);
            var getposOnLineB = B.GetposOnLine(lineB);
            var getposOnLineC = c.GetposOnLine(this);
            var getposOnLineD = d.GetposOnLine(this);
            return getposOnLineA switch
            {
                Pt2LinePos.On => getposOnLineC != getposOnLineD,
                var posA when posA != getposOnLineB => getposOnLineC != getposOnLineD,
                _ => false
            };
        }

        public bool IsCrossAnother(TwoDVectorLine lineB)
        {
            var c = lineB.A;
            var d = lineB.B;
            var getposOnLineA = A.GetposOnLine(lineB);
            var getposOnLineB = B.GetposOnLine(lineB);
            var getposOnLineC = c.GetposOnLine(this);
            var getposOnLineD = d.GetposOnLine(this);
            return getposOnLineA switch
                   {
                       Pt2LinePos.Left => getposOnLineB == Pt2LinePos.Right,
                       Pt2LinePos.Right => getposOnLineB == Pt2LinePos.Left,
                       _ => false
                   }
                   &&
                   getposOnLineC switch
                   {
                       Pt2LinePos.Left => getposOnLineD == Pt2LinePos.Right,
                       Pt2LinePos.Right => getposOnLineD == Pt2LinePos.Left,
                       _ => false
                   };
        }
    }
}