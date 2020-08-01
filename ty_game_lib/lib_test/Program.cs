﻿#nullable enable
using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;
using game_stuff;

namespace lib_test
{
    internal class Program
    {
        private static void Main(string[] args)
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
            var crossPoint = ab.CrossPointForWholeLine(cd);
            var point = ad.CrossPointForWholeLine(bc);
            SomeTools.LogPt(crossAnotherPoint);
            SomeTools.LogPt(anotherPoint);
            SomeTools.LogPt(crossPoint);
            SomeTools.LogPt(point);
            Console.Out.WriteLine("Block test!!!");

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

            var ptt = new TwoDPoint(2f, -1.717f);
            var twoDPoints = new[] {pt1, pt2, pt3, pt4, pt5, pt6};
//            foreach (var twoDPoint in twoDPoints)
//            {
//                Console.Out.WriteLine("res:" + twoDPoint.X);
//            }

            var poly = new Poly(twoDPoints);

//            var blockShapes = poly.GenBlockShapes(0.2f, false);
//            foreach (var blockShape in blockShapes)
//            {
//                var twoDPoint = blockShape.GetEndPt();
//                Console.Out.WriteLine("l1 raw end pt::X::" + twoDPoint.X + "   Y::" + twoDPoint.Y);
//            }


//            var genByPoly = poly.GenWalkBlockByPoly(0.2f, 2, false);
//            var inBlock1 = genByPoly.CoverPoint(ptt);
//            if (genByPoly.QSpace != null) Console.Out.WriteLine(genByPoly.QSpace.OutZones());
//            else
//            {
//                Console.Out.WriteLine("all block");
//            }
//
//            Console.WriteLine("!!!!" + inBlock1);

            var pp1 = new TwoDPoint(2f, -0.5f);
            var pp2 = new TwoDPoint(2.5f, -1f);
            var pp3 = new TwoDPoint(2, -1.5f);
            var pp4 = new TwoDPoint(1.5f, -1f);
            var dPoints = new[] {pp1, pp2, pp3, pp4};
            var poly1 = new Poly(dPoints);
            var tuples = new List<(Poly, bool)>
            {
                (poly, false), (poly1, true)
            };

//            var genBlockShapes = poly1.GenBlockShapes(0.2f, true);
//            foreach (var blockShape in genBlockShapes)
//            {
//                var twoDPoint = blockShape.GetEndPt();
//                Console.Out.WriteLine("raw end pt::X"+ twoDPoint.X+ "   Y::"+twoDPoint.Y);
//
//            }
//
//            var genWalkBlockByPoly = poly1.GenWalkBlockByPoly(0.2f,100,true);
//            var outZones = genWalkBlockByPoly.QSpace.OutZones();
//            Console.Out.WriteLine("zones!!!:::"+outZones);

            WalkBlock genWalkBlockByPolys = SomeTools.GenWalkBlockByPolys(tuples, 0.2f, 7);
            Console.Out.WriteLine("ResIsBlockIN?" + genWalkBlockByPolys.IsBlockIn);
            if (genWalkBlockByPolys.QSpace != null) Console.Out.WriteLine(genWalkBlockByPolys.QSpace.OutZones());
            else
                Console.Out.WriteLine("all block");

            var inBlock2 = genWalkBlockByPolys.CoverPoint(ptt);

            Console.Out.WriteLine("!!2!!" + inBlock2);
            Console.Out.WriteLine("config test~~~~~");

            var configDictionaries = new ConfigDictionaries();
            var configDictionariesBullets = configDictionaries.bullets;
            foreach (KeyValuePair<string, bullet> configDictionariesBullet in configDictionariesBullets)
            {
                var key = configDictionariesBullet.Key;
                Console.Out.WriteLine($"{key}");
                foreach (var keyValuePair in configDictionariesBullet.Value.FailActBuffConfigToSelf)
                {
                    Console.Out.WriteLine($"{keyValuePair.size}");
                }
            }

            var testPlayGround = TestStuff.TestPlayGround();
            foreach (var keyValuePair in testPlayGround.Item2)
            {
                Console.Out.WriteLine($"{keyValuePair.Key}");
                var charInitMsgs = keyValuePair.Value;
                foreach (var charInitMsg in charInitMsgs)
                {
                    var gId = charInitMsg.GId;
                    var logPt = charInitMsg.Pos.LogPt();
                    var twoDVector = charInitMsg.Aim.LogVector();
                    Console.Out.WriteLine($"{gId}::{logPt}::{twoDVector}");
                }
            }

            var pi = MathTools.Pi() / 6;
            var operate = new Operate(null, null,
                new TwoDVector(MathTools.Cos(pi), MathTools.Sin(pi)));
            var logVector = operate.Move?.LogVector();
            Console.Out.WriteLine($"move op ::{logVector}");
            var dictionary = new Dictionary<int, Operate>() {{1, operate}};

            var playGround = testPlayGround.Item1;
            var playGroundGoATick = playGround.PlayGroundGoATick(dictionary);
            foreach (var keyValuePair in playGroundGoATick.Item2)
            {
                foreach (var charTickMsg in keyValuePair.Value)
                {
                    var twoDPoint = charTickMsg.Pos;
                    var logPt = twoDPoint.LogPt();
                    var log = charTickMsg.Aim.Log();
                    Console.Out.WriteLine($"a tick after pos {logPt} aim {log}");
                    
                    
                }
            }
        }
    }
}

