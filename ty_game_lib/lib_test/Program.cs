#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using collision_and_rigid;
using cov_path_navi;
using game_config;
using game_stuff;

namespace lib_test
{
    internal class Program
    {
        void BlockTest(WalkBlock walkBlock, TwoDPoint ptt)
        {
            Console.Out.WriteLine("Block test!!!");
            Console.Out.WriteLine("ResIsBlockIN?" + walkBlock.IsBlockIn);
            Console.Out.WriteLine(walkBlock.QSpace.OutZones());

            var inBlock2 = walkBlock.RealCoverPoint(ptt);

            var sPt = new TwoDPoint(-0.1f, -1.45f);
            var ePt = new TwoDPoint(0.2f, -1.55f);

            var pushOutToPt = walkBlock.PushOutToPt(sPt, ePt);

            Console.Out.WriteLine($"push out pt {pushOutToPt.pt}");
            Console.Out.WriteLine("!!2!!" + inBlock2);

            TwoDPoint a = new TwoDPoint(0f, 0f);
            TwoDPoint b = new TwoDPoint(1f, 1f);
            TwoDPoint c = new TwoDPoint(0.5f, 0.5f);
            TwoDPoint d = new TwoDPoint(0f, 1f);
            var bLine = new TwoDVectorLine(a, b);
            var cLine = new TwoDVectorLine(c, d);
            var isGoTrough = cLine.IsGoTrough(bLine);
            Console.Out.WriteLine($"is go tr {isGoTrough}");
        }

        private static void LineTest()
        {
            Console.Out.WriteLine("Line test!!");

            var aPoint = new TwoDPoint(1.5f, 1.2f);
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
                $"ab cross cd{crossAnotherPoint}\nad cross bc{anotherPoint}\nab cross line cd{crossPoint}" +
                $"\nad cross line bc{point}");
        }

        private static void PathTest(WalkBlock genWalkBlockByPolys)
        {
            Console.Out.WriteLine("path test~~~~~");

            var allIBlocks = genWalkBlockByPolys.QSpace?.GetAllIBlocks();

            var aggregate = allIBlocks!.Aggregate("", (s, x) => s + x.ToString() + "\n");

            if (allIBlocks != null)
            {
                Console.Out.WriteLine($"blocks have {allIBlocks.Count} blocks \n are::{aggregate}");

                var genFromBlocks = PathTop.GenFromBlocks(allIBlocks.ToList());

                var aggregate1 =
                    genFromBlocks.Aggregate("",
                        (s, list) => s + "b num:: " + list.Count + "\n" +
                                     list.Aggregate("", (s2, x) => s2 + x.ToString() + "\n") + "\n");

                Console.Out.WriteLine($"linked blocks is \n{aggregate1}");

                var genBlockUnits = PathTop.GenBlockUnits(genFromBlocks, genWalkBlockByPolys.IsBlockIn);

                var s1 = genBlockUnits.Aggregate("", (s, x) => s + x);

                Console.Out.WriteLine($"WalkAreaBlocks \n{s1}");

                var walkAreas = genBlockUnits.Select(x => x.GenWalkArea());

                var continuousWalkAreas = walkAreas as ContinuousWalkArea[] ?? walkAreas.ToArray();
                var ss2 = continuousWalkAreas.Aggregate("", (s, x) => s + x);

                Console.Out.WriteLine($"WalkAreas \n{ss2}");

                var startId = -1;
                var pathNodeCovPolygons = continuousWalkAreas.SelectMany(x => x.ToCovPolygons(ref startId));

                var aggregate2 = pathNodeCovPolygons.Aggregate("", (s, x) => s + x + "\n");
                Console.Out.WriteLine($"finally~~~~~\n{aggregate2}");
            }

            var pathTop = new PathTop(genWalkBlockByPolys);
            Console.Out.WriteLine($"\n\n pathTop is\n{pathTop}");
            Console.Out.WriteLine($"\n\n pathTop ok");
            var startPt = new TwoDPoint(-7f, -5f);
            var endPt = new TwoDPoint(6, 0f);

            var findAPathById = pathTop.FindAPathById(0, 6, startPt, endPt);

            var aggregate3 =
                findAPathById.Aggregate("", (s, x) => s + "=>>" + x.Item2?.ToString() + "||" + x.Item1);


            var findAPathByPoint = pathTop.FindAPathByPoint(startPt, endPt);
            var aggregate4 =
                findAPathByPoint.Aggregate("", (s, x) => s + "=>>" + x.Item2?.ToString() + "||" + x.Item1);
            Console.Out.WriteLine($"path::{aggregate4}");
            var twoDVectorLines = findAPathByPoint.Select(x => x.Item2);
            var goPts = PathTop.GetGoPts(startPt, endPt, twoDVectorLines.ToArray());
            var s3 = goPts.Aggregate("", (s, x) => s + "=>" + x.ToString());
            Console.Out.WriteLine($"way Points are {s3}");
        }

        private static void Main(string[] args)
        {
           
        }

        private static void LogCPos(ImmutableDictionary<int, ImmutableHashSet<IPerceivable>> item2)
        {
            foreach (var keyValuePair in item2)
            {
                foreach (var charTickMsg in keyValuePair.Value.OfType<ICanBeSaw>().Select(x => x.GenTickMsg())
                    .OfType<CharTickMsg>())
                {
                    var twoDPoint = charTickMsg.Pos;
                    var logPt = twoDPoint.LogPt();
                    var log = charTickMsg.Aim.ToString();
                    var isOnHit = charTickMsg.IsBeHit;
                    var isStun = charTickMsg.IsStun;
                    var skillLaunch = charTickMsg.SkillLaunch == null ? "null" : charTickMsg.SkillLaunch.ToString();
                    Console.Out.WriteLine(
                        $"{keyValuePair.Key}go a tick  get: Player {charTickMsg.Gid} , pos {logPt}, aim {log}, " +
                        $"speed :{charTickMsg.Speed}, is on hit::{isOnHit} , is stun :: {isStun},skill launch {skillLaunch}");
                }
            }
        }
    }
}