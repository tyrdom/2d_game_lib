#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using cov_path_navi;
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
            Console.Out.WriteLine(
                $"ab cross cd{crossAnotherPoint?.Log()}\nad cross bc{anotherPoint?.Log()}\nab cross line cd{crossPoint?.Log()}" +
                $"\nad cross line bc{point?.Log()}");


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

            var poly = TestStuff.TestPoly();

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
            var poly1 = TestStuff.TestPoly2();
            var poly2 = TestStuff.TestPoly3();
            var poly3 = new Poly(new[]
                {new TwoDPoint(2, 10), new TwoDPoint(4, 10), new TwoDPoint(4, -10), new TwoDPoint(2, -10),});

            var poly4 = new Poly(new[]
                {new TwoDPoint(-2, 2), new TwoDPoint(-4, 2), new TwoDPoint(-4, 4), new TwoDPoint(-2, 4)});

            var poly5 = new Poly(new[]
            {
                new TwoDPoint(-2, -2), new TwoDPoint(-2, -4), new TwoDPoint(-4, -4), new TwoDPoint(-4, -2)
            });

            var tuples = new List<(Poly, bool)>
            {
                (poly, false), (poly3, true), (poly4, true), (poly5, true)
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

            var genWalkBlockByPolys = SomeTools.GenWalkBlockByPolygons(tuples, 1f, 6);
            Console.Out.WriteLine("ResIsBlockIN?" + genWalkBlockByPolys.IsBlockIn);
            if (genWalkBlockByPolys.QSpace != null) Console.Out.WriteLine(genWalkBlockByPolys.QSpace.OutZones());
            else
                Console.Out.WriteLine("all block");

            var inBlock2 = genWalkBlockByPolys.RealCoverPoint(ptt);

            var sPt = new TwoDPoint(-0.1f, -1.45f);
            var ePt = new TwoDPoint(0.2f, -1.55f);

            var pushOutToPt = genWalkBlockByPolys.PushOutToPt(sPt, ePt);

            Console.Out.WriteLine($"push out pt {pushOutToPt?.Log()}");
            Console.Out.WriteLine("!!2!!" + inBlock2);

            TwoDPoint a = new TwoDPoint(0f, 0f);
            TwoDPoint b = new TwoDPoint(1f, 1f);
            TwoDPoint c = new TwoDPoint(0.5f, 0.5f);
            TwoDPoint d = new TwoDPoint(0f, 1f);
            var bLine = new TwoDVectorLine(a, b);
            var cLine = new TwoDVectorLine(c, d);
            var isGoTrough = cLine.IsGoTrough(bLine);
            Console.Out.WriteLine($"is go tr {isGoTrough}");

            Console.Out.WriteLine("path test~~~~~");

            var allIBlocks = genWalkBlockByPolys.QSpace?.GetAllIBlocks();

            var aggregate = allIBlocks.Aggregate("", (s, x) => s + x.Log() + "\n");

            if (allIBlocks != null)
            {
                Console.Out.WriteLine($"blocks have {allIBlocks.Count} blocks \n are::{aggregate}");

                var genFromBlocks = PathTop.GenFromBlocks(allIBlocks.ToList());

                var aggregate1 =
                    genFromBlocks.Aggregate("",
                        (s, list) => s + "b num:: " + list.Count + "\n" +
                                     list.Aggregate("", (s2, x) => s2 + x.Log() + "\n") + "\n");

                Console.Out.WriteLine($"linked blocks is \n{aggregate1}");

                var genBlockUnits = PathTop.GenBlockUnits(genFromBlocks, genWalkBlockByPolys.IsBlockIn);

                var s1 = genBlockUnits.Aggregate("", (s, x) => s + x);

                Console.Out.WriteLine($"WalkAreaBlocks \n{s1}");

                var walkAreas = genBlockUnits.Select(x => x.GenWalkArea());

                var continuousWalkAreas = walkAreas as ContinuousWalkArea[] ?? walkAreas.ToArray();
                var ss2 = continuousWalkAreas.Aggregate("", (s, x) => s + x);

                Console.Out.WriteLine($"WalkAreas \n{ss2}");

                int startId = -1;
                var pathNodeCovPolygons = continuousWalkAreas.SelectMany(x => x.ToCovPolygons(ref startId));

                var aggregate2 = pathNodeCovPolygons.Aggregate("", (s, x) => s + x + "\n");
                Console.Out.WriteLine($"finally~~~~~\n{aggregate2}");
            }

            var pathTop = new PathTop(genWalkBlockByPolys);
            Console.Out.WriteLine($"\n\n pathTop is\n{pathTop}");
            var twoDPoint = new TwoDPoint(-2.1f, 0);
            var findAPathById = pathTop.FindAPathById(0, 6, twoDPoint);

            var aggregate3 = findAPathById.Aggregate("", (s, x) => s + "=>>" + x.Item2?.Log() + "||" + x.Item1);
            Console.Out.WriteLine($"path::{aggregate3}");

            return;
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

            Console.Out.WriteLine("game test~~~~~");

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


            var pi = MathTools.Pi();
            var aa = pi;
            var bb = pi / 2f;
            var aVector = new TwoDVector(MathTools.Cos(aa), MathTools.Sin(aa));
            var bVector = new TwoDVector(MathTools.Cos(bb), MathTools.Sin(bb));
            var cVector = new TwoDVector(-1, 0);

            var move = new Operate(null, null,
                aVector);
            var skill = new Operate(null, SkillAction.Op2, null);
            var skill2 = new Operate(null, SkillAction.Op1, null);
            var turn = new Operate(bVector, null, null);
            var turn2 = new Operate(aVector, null, null);
            // var logVector = operate.Move?.LogVector();
            // Console.Out.WriteLine($"move op ::{logVector}");
            var operate1 = new Operate(cVector, null, null);
            var operates1 = new[] {operate1, turn2, turn, turn2, operate1};
            var dictionary = new Dictionary<int, Operate> {{1, skill}};
            var dictionary2 = new Dictionary<int, Operate> {{1, skill2}};
            var enumerable = operates1.Select(x => new Dictionary<int, Operate> {{1, x}}).ToArray();

            var playGround = testPlayGround.Item1;
            // var playGroundGoATick = playGround.PlayGroundGoATick(dictionary);
            //
            // LogCPos(playGroundGoATick.Item2);

            var operates = new Dictionary<int, Operate> {{1, operate1}};

            // var (item1, item2) = playGround.PlayGroundGoATick(operates);
            // LogCPos(item2);
            var range = Enumerable.Range(1, 100).ToArray();
            foreach (var i in range)
            {
                var (_, item3) =
                    i == 1
                        ? playGround.PlayGroundGoATick(operates)
                        : i < 30
                            ? playGround.PlayGroundGoATick(dictionary)
                            : playGround.PlayGroundGoATick(dictionary2);

                // if (i == 1) playGround.PlayGroundGoATick(operates);
                //
                // var (_, item3) = (i % 20 != 1 && i < 60)
                //     ? playGround.PlayGroundGoATick(dictionary)
                //     : playGround.PlayGroundGoATick(operates);
                Console.Out.WriteLine($"{i}");
                LogCPos(item3);
            }
        }

        public static void LogCPos(Dictionary<int, IEnumerable<CharTickMsg>> item2)
        {
            foreach (var keyValuePair in item2)
            {
                foreach (var charTickMsg in keyValuePair.Value)
                {
                    var twoDPoint = charTickMsg.Pos;
                    var logPt = twoDPoint.LogPt();
                    var log = charTickMsg.Aim.Log();
                    var isOnHit = charTickMsg.IsBeHit;
                    var isStun = charTickMsg.IsStun;
                    var skillLaunch = charTickMsg.SkillLaunch == null ? "null" : charTickMsg.SkillLaunch.ToString();
                    Console.Out.WriteLine(
                        $"{keyValuePair.Key}go a tick  get: Player {charTickMsg.Gid} , pos {logPt}, aim {log}, " +
                        $"speed :{charTickMsg.Speed}, is on hit::{isOnHit?.Log()} , is stun :: {isStun},skill launch {skillLaunch}");
                }
            }
        }
    }
}