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
            Console.Out.WriteLine("Line test!!");

            var aPoint = new TwoDPoint(1.5f, 1);
            var bPoint = new TwoDPoint(1, 1);

            var cPoint = new TwoDPoint(0, 0);
            var dPoint = new TwoDPoint(1, 0);

            var ab = new TwoDVectorLine(aPoint, bPoint);
            var cd = new TwoDVectorLine(cPoint, dPoint);
            var ad = new TwoDVectorLine(aPoint, dPoint);
            var bc = new TwoDVectorLine(bPoint, cPoint);
            var crossAnotherPoint = ab.CrossAnotherPointInLinesIncludeEnds(cd);
            var anotherPoint = ad.CrossAnotherPointInLinesIncludeEnds(bc);
            var crossPoint = ab.CrossPoint(cd);
            var point = ad.CrossPoint(bc);
            SomeTools.LogPt(crossAnotherPoint);
            SomeTools.LogPt(anotherPoint);
            SomeTools.LogPt(crossPoint);
            SomeTools.LogPt(point);
            Console.WriteLine("Block test!");
//
//            var pt1 = new TwoDPoint(1.5f, 1.3f);
//            var pt2 = new TwoDPoint(2f, 2.8f);
//            var pt3 = new TwoDPoint(3.6f, 0.9f);
//            var pt4 = new TwoDPoint(2.5f, -1.7f);

            var pt1 = new TwoDPoint(0.0f, 0.0f);
            var pt2 = new TwoDPoint(1.0f, 1.0f);
            var pt3 = new TwoDPoint(2f, 0f);
            var pt4 = new TwoDPoint(3.0f, 1f);
            var pt5 = new TwoDPoint(4.0f, 0f);

            var pt6 = new TwoDPoint(2.0f, -2.0f);

            var ptt = new TwoDPoint(0f, -2f);
            var twoDPoints = new TwoDPoint[] {pt1, pt2, pt3, pt4, pt5, pt6};
            foreach (var twoDPoint in twoDPoints)
            {
                Console.Out.WriteLine("res:" + twoDPoint.X);
            }

            var poly = new Poly(twoDPoints);
            var genByPoly = poly.GenByPoly(5.5f, 20);
            var inBlock1 = genByPoly.InBlock(ptt);
//          Console.Out.WriteLine(genByPoly.QSpace.outZones());
            Console.WriteLine("!!!!" + inBlock1);
        }
    }
}