﻿using System;
using System.Collections;
using System.Collections.Generic;
using ty_game_lib;

namespace lib_test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var pts = new TwoDPoint(1.5f, 1.3f);


            var either = new Either<int, string>(5);

            Console.WriteLine(either.left + "  " + (either.right == null));

            var aabbBoxes = new AabbBox?[4];
            Console.WriteLine("!!" + aabbBoxes[0]);

            List<AabbBox> aabbBoxes2 = new List<AabbBox>();
            aabbBoxes2.Add(aabbBoxes[1]);
            var aabbBoxes2Count = aabbBoxes2.Count;
            Console.WriteLine(aabbBoxes2Count);
           

        }
    }
}