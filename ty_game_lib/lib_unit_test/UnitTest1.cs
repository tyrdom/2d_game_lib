using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using collision_and_rigid;
using cov_path_navi;
using game_config;
using game_stuff;
using NUnit.Framework;
using rogue_game;

namespace lib_unit_test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            rogue_game.RogueLocalConfig.LoadConfig();
        }

        [Test]
        public void Test1()
        {
            var genAChapterMap = rogue_chapter_maker.ChapterMapTop.GenAChapterMap(7, 3, 1, 1, false, true, 2, 1, 3, 2);
            var selectMany = genAChapterMap.PointMaps.SelectMany(x => x.ToString());
            var s = new string(selectMany.ToArray());
            Assert.Pass(s);
        }

        [Test]
        public void Test2()
        {
            var genAChapterMap =
                rogue_chapter_maker.ChapterMapTop.GenAChapterMap(7, 3, 1, 1, false, false, 2, 1, 4, 2);

            Assert.Pass(genAChapterMap.ToString());
        }

        [Test]
        public void PathTest1()
        {
            var aggregate = ABlock()?.Aggregate("Path Test1:\n", (s, x) => s + x.ToString() + "\n") +
                            "Path Test1 end:\n";

            Assert.Pass(aggregate);
        }

        [Test]
        public void PathTest2()
        {
            if (ABlock() != null)
            {
                var genFromBlocks = PathTop.GenFromBlocks(ABlock()?.ToList() ?? new List<IBlockShape>());

                var aggregate1 =
                    genFromBlocks.Aggregate("",
                        (s, list) => s + "b num:: " + list.Count + "\n" +
                                     list.Aggregate("", (s2, x) => s2 + x.ToString() + "\n") + "\n");

                Assert.Pass(aggregate1);
            }
            else
            {
                Assert.Pass("null");
            }
        }

        [Test]
        public void PathTestF()
        {
            var pathTest = PathTest(TestWb());
            Assert.Pass(pathTest);
        }

        [Test]
        public void ConfigTest()
        {
            var keyValuePair = CommonConfig.Configs.map_rawss.Values.First();
            var id = keyValuePair.WalkRawMap.First().First().x;
            Assert.Pass(id.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void RGameInitTest()
        {
            var characterBodies = Players();
            var genByConfig = RogueGame.GenByConfig(characterBodies, characterBodies.First());
            Assert.Pass(genByConfig.PlayerLeaderGid.ToString());
        }

        [Test]
        public void PassiveTest()
        {
            var floats = new[] {1.2f, 0f, 0f, 4.1f, 5.4f, 4f, 4f, 4f, 4f};
            Span<float> span = floats;
            var vector = new Vector<float>(floats);
            var hashCode = vector[1];
            Assert.Pass($"ok~ok~ok~~~~{hashCode}");
        }

        private static HashSet<CharacterBody> Players()
        {
            var characterInitDataS = new[] {TestStuff.TestPlayer1(), TestStuff.TestPlayer2()};
            var characterBodies = characterInitDataS.Select(x => x.GenCharacterBody(TwoDPoint.Zero()));
            return characterBodies.IeToHashSet();
        }

        private static HashSet<IBlockShape>? ABlock()
        {
            var allIBlocks = TestWb().QSpace.GetAllIBlocks();
            return allIBlocks;
        }

        private static WalkBlock TestWb()
        {
            var poly = TestStuff.TestPoly();
            var poly1 = TestStuff.TestPoly2();
            var poly2 = TestStuff.TestPoly3();
            var poly3 = new Poly(new[]
                {new TwoDPoint(2, 1), new TwoDPoint(4, 1), new TwoDPoint(4, -1), new TwoDPoint(2, -1),});

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

            var genWalkBlockByPolys = SomeTools.GenWalkBlockByPolygons(tuples, 1f, 6);
            return genWalkBlockByPolys;
        }

        private static string PathTest(WalkBlock genWalkBlockByPolys)
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

                var genBlockUnits = PathTop.GenBlockUnits(genFromBlocks, genWalkBlockByPolys.IsBlockIn).ToList();

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
                findAPathById.Aggregate("", (s, x) => s + "=>>" + x.Item2 + "||" + x.Item1);


            var findAPathByPoint = pathTop.FindAPathByPoint(startPt, endPt).ToArray();
            var aggregate4 =
                findAPathByPoint.Aggregate("", (s, x) => s + "=>>" + x.Item2?.ToString() + "||" + x.Item1);
            Console.Out.WriteLine($"path::{aggregate4}");
            var twoDVectorLines = findAPathByPoint.Select(x => x.Item2);
            var goPts = PathTop.GetGoPts(startPt, endPt, twoDVectorLines.ToArray());
            var s3 = goPts.Aggregate("", (s, x) => s + "=>" + x);
            Console.Out.WriteLine($"way Points are {s3}");
            return s3;
        }
    }
}