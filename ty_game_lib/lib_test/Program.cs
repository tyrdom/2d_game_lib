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
            

            var either = new Either<int,string>(5);
            
            Console.WriteLine(either.left +"  "+ (either.right==null));
        }
    }
}