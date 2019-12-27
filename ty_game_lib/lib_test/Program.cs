#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using ty_game_lib;

namespace lib_test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Block test!");
//
            var pt1 = new TwoDPoint(1.5f, 1.3f);
            var pt2 = new TwoDPoint(2f, 2.8f);
            var pt3 = new TwoDPoint(3.6f, 0.9f);
            var pt4 = new TwoDPoint(2.5f, -1.7f);

//            var pt1 = new TwoDPoint(0.0f, 1.0f);
//            var pt2 = new TwoDPoint(1.0f, 2.0f);
//            var pt3 = new TwoDPoint(2.0f, 1f);
//            var pt4 = new TwoDPoint(1.0f, 0f);

            var twoDPoints = new TwoDPoint[] {pt1, pt2, pt3, pt4};
            foreach (var twoDPoint in twoDPoints)
            {
                Console.Out.WriteLine("res:" + twoDPoint.X);
            }

            var poly = new Poly(twoDPoints);
            var genByPoly = poly.GenByPoly(2.5f, 3);
            var inBlock1 = genByPoly.inBlock(pt4);
//            Console.Out.WriteLine(genByPoly.QSpace.outZones());
            Console.WriteLine("!!!!" + inBlock1);
        }
    }
}