using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using collision_and_rigid;
using cov_path_navi;
using game_bot;
using game_config;
using game_stuff;
using NUnit.Framework;
using rogue_chapter_maker;
using rogue_game;

namespace lib_unit_test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            CommonConfig.LoadConfig();
            RogueLocalConfig.ReLoadP();
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
        public void SightTest()
        {
            var sightMap = StuffLocalConfig.PerLoadMapConfig[1].SightMap;
            var ss = sightMap?.Lines.ToString(0) ?? "";
            var twoDVectorLine = new TwoDVectorLine(new TwoDPoint(9.8f, 7.2f), new TwoDPoint(2.5f, 15f));
            var isBlockSightLine = sightMap?.IsBlockSightLine(twoDVectorLine) ?? false;
            Assert.Pass(ss + "\n" + isBlockSightLine + "\n");
        }

        [Test]
        public void TestDir()
        {
            var currentDirectory = Environment.CurrentDirectory;
            for (var i = 0; i < 4; i++)
            {
                var x = currentDirectory.LastIndexOf("\\", StringComparison.Ordinal);
                currentDirectory = currentDirectory[..x];
            }

            Assert.Pass(currentDirectory);
        }

        [Test]
        public void Test2()
        {
            var genAChapterMap =
                rogue_chapter_maker.ChapterMapTop.GenAChapterMap(0, 6, 1, 0, false, false, 2, 1, 4, 2);

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
        public void GenBulletDic()
        {
            var genById = Bullet.GenById(bullet_id.test_ss_l_1_b_1);
            var bulletBox = genById.SizeToBulletCollision[size.small];
            var b = bulletBox.BulletShape is SimpleBlocks ss ? ss.ToString() : "";
            var s = bulletBox.Zone.ToString();
            Assert.Pass(s + b);
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
            var s = BotLocalConfig.NaviMapPerLoad[2][size.small].ToString();
            Assert.Pass(s);
        }

        [Test]
        public void RGameInitTest()
        {
            var characterBodies = Players();
            var genByConfig = RogueGame.GenByConfig(characterBodies, characterBodies.First().Gid, out var gameRespS);
            Assert.Pass(genByConfig.PlayerLeaderGid.ToString());
        }

        [Test]
        public void PassiveTest()
        {
            var floats = new[] { 1.2f, 0f, 0f, 4.1f, 5.4f, 4f, 4f, 4f, 4f };
            Span<float> span = floats;
            var vector = new Vector<float>(floats);
            var hashCode = vector[1];
            Assert.Pass($"ok~ok~ok~~~~{hashCode}");
        }

        [Test]
        public void BotTest()
        {
            var pointMap = new PointMap(MapType.Small, 1, 1, 1, 1, new Slot(0, 0));
            var genEmptyPveMap = PveMap.GenEmptyPveMap(pointMap, 1, 1, new[] { 4, 8 }, new int[] { });
            var botTeam = new BotTeam();
            botTeam.SetNaviMaps(genEmptyPveMap.PlayGround.ResMId);
            genEmptyPveMap.SpawnNpcWithBot(new Random(), botTeam, 5);
            genEmptyPveMap.PlayGroundGoATick(new Dictionary<int, Operate>());

            for (var i = 0; i < 10000; i++)
            {
                Console.Out.WriteLine($"op num {botTeam.TempOperate.Count}");

                var keyValuePairs = botTeam.TempOperate.Where(botTeamTempOpThink =>
                    botTeamTempOpThink.Value != null).ToDictionary(p => p.Key, p => p.Value);
                Console.Out.WriteLine($"~~~~{keyValuePairs.FirstOrDefault().Value?.GetMove() ?? TwoDVector.Zero()}");
                var playGroundGoTickResult = genEmptyPveMap.PlayGroundGoATick(keyValuePairs);
                botTeam.AllBotsGoATick(playGroundGoTickResult);
            }

            Assert.Pass($"fin");
        }

        private static HashSet<CharacterInitData> Players()
        {
            var characterInitDataS = new[] { TestStuff.TestPlayer1(), TestStuff.TestPlayer2() };

            return characterInitDataS.IeToHashSet();
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
                { new TwoDPoint(2, 1), new TwoDPoint(4, 1), new TwoDPoint(4, -1), new TwoDPoint(2, -1), });

            var poly4 = new Poly(new[]
                { new TwoDPoint(-2, 2), new TwoDPoint(-4, 2), new TwoDPoint(-4, 4), new TwoDPoint(-2, 4) });

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

        [Test]
        public void NpcTest()
        {
            var genById = BattleNpc.GenById(5, 1111, 2, new Random(), 2, out _);

            Assert.Pass($"fin");
        }

        [Test]
        public void WalkBlockPushTest()
        {
            var configDictionaries = new ConfigDictionaries();
            var mapRaws = configDictionaries.map_rawss[15];

            var mapRawsWalkRawMap = mapRaws.WalkRawMap;
            var enumerable = mapRawsWalkRawMap.Select(x => x.GenPoly()).ToArray();
            var mapByPolys = WalkMap.CreateMapByPolys(enumerable.PloyListMark());
            var walkBlock = mapByPolys.SizeToEdge[size.small];
            var twoDPoint = new TwoDPoint(-0.06930432f, 6.693746f);
            var twoDPoint2 = new TwoDPoint(-0.7251987f, 6.235703f);
            var (isHitWall, pt) = walkBlock.PushOutToPt(twoDPoint, twoDPoint2);
            var realCoverPoint = walkBlock.RealCoverPoint(pt);
            //  Last[-0.06930432|6.693746],Now [-0.7251987|6.235703] = [-0.7251987|6.235703]

            Assert.Pass($"fin mapRaw {mapRaws.info}  hitWall:{isHitWall} pt:{pt} Cover:{realCoverPoint} \n");
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
            var twoDVectorLines = findAPathByPoint.Where(x => x.gothroughLine != null).Select(x => x.Item2);
            var goPts = PathTop.GetGoPts(startPt, endPt, twoDVectorLines.ToArray());
            var s3 = goPts.Aggregate("", (s, x) => s + "=>" + x);
            Console.Out.WriteLine($"way Points are {s3}");
            return s3;
        }
    }
}