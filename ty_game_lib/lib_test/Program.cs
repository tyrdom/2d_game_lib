using System;
using ty_game_lib;

namespace lib_test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var pts = new TwoDPoint(1.5f, 1.3f);
            Console.WriteLine(pts.X.ToString() +"  "+ pts.Y.ToString());
            
        }
    }
}