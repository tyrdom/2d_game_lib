using System.Runtime.Intrinsics.X86;

namespace ty_game_lib
{
    public class TwoDVectorLine
    {
     public   TwoDVectorLine(TwoDPoint a, TwoDPoint b)
        {
            this.a = a;
            this.b = b;
        }


        public TwoDPoint a;
        public TwoDPoint b;

        public TwoDVector getVector()
        {
            return new TwoDVector(this.b.x - this.a.x, this.b.y - this.a.y);
        }
    }
}