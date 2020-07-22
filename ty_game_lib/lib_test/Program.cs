#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using collision_and_rigid;
using game_config;

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
            Console.WriteLine("Block test!!!");

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

            Console.WriteLine("!!2!!" + inBlock2);
            Console.Out.WriteLine("config test~~~~~");

            foreach (var key in Content.push_buffs)
            {
                Console.Out.WriteLine($"key::{key.Key}");
                Console.Out.WriteLine($"pushType::{key.Value.PushType.ToString()}");
            }

            var twoSToSeePertick = Content.other_configs[1].two_s_to_see_pertick;
            Console.Out.WriteLine($"other_c:::{twoSToSeePertick}");

            foreach (var bodysKey in Content.bodys.Keys)
            {
                Console.Out.WriteLine($"{bodysKey.ToString()}");
            }


            var mainModuleFileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var sep = Path.DirectorySeparatorChar;
            var name = Content.fishs.Values.First().GetType().ToString();
            var fileInfo = new FileInfo($".{sep}Bytes{sep}aaa.bytes");
            var fileInfo2 = new FileInfo($".{sep}Bytes{sep}{name}.bytes");
            // Console.Out.WriteLine($"{sep}\n{fileInfo.DirectoryName},\n{mainModuleFileName}");
            if (!fileInfo.Directory.Exists) fileInfo.Directory.Create();
         

            // GameConfigTools.SaveDict(Content.fishs, fileInfo2);

            var immutableDictionary = GameConfigTools.LoadDictByByte<int, fish>(fileInfo);
            foreach (var keyValuePair in immutableDictionary)
            {
                Console.Out.WriteLine($"{keyValuePair.Key}");
            }


            foreach (var dictionary in Content.all_Immutable_dictionary)
            {
                var type1 = (from object? key in dictionary.Keys select key?.GetType()).FirstOrDefault();
                var type2 = (from object? key in dictionary.Values select key?.GetType()).FirstOrDefault();
                Console.Out.WriteLine($"{type1}");
                Console.Out.WriteLine($"{type2}");
                var cName = type2.ToString();
                var fileInfo3 = new FileInfo($".{sep}Bytes{sep}{cName}.bytes");

                //save dictionary
                var methodInfo = typeof(GameConfigTools).GetMethod("SaveDictByByte");
                var makeGenericMethod = methodInfo?.MakeGenericMethod(type1, type2);
                makeGenericMethod?.Invoke(null, new object[] {dictionary, fileInfo3});
                Console.Out.WriteLine($"{dictionary.GetType()} saved");
                // var method = typeof(GameConfigTools).GetMethod("LaodDict");
                // var genericMethod = method?.MakeGenericMethod(type1, type2);
                // var invoke = (IDictionary)genericMethod?.Invoke(null, new object?[] {fileInfo3})!;
                // foreach (DictionaryEntry o in invoke)
                // {
                //     Console.Out.WriteLine($"{o}");
                // }
            }

            // GameConfigTools.LoadDict(fileInfo2, out ImmutableDictionary<int, bad_words> a);
            // foreach (var keyValuePair
            //     in a)
            // {
            //     Console.Out.WriteLine($"{keyValuePair.Key}");
            // }
            //
            // Content.bad_wordss = a;
        }
    }
}