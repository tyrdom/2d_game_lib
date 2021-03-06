using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;
using game_config;
using game_stuff;

namespace lib_unit_test
{
    public static class TestStuff
    {
        public static PlayGround TestPlayGround()
        {
            var testInitData = TestInitData();
            var twoDPoints = testInitData.TeamToStartPt.ToDictionary(p => p.Key, p => p.Value.GenPt())
                .ToImmutableDictionary();
            var initPlayGround = PlayGround.InitPlayGround(new[] {TestPlayer1(), TestPlayer2()},
                testInitData, 1, 0);

            return initPlayGround;
        }

        public static CharacterInitData TestPlayer1()
        {
            var keyValuePair = CommonConfig.Configs.weapons.First();
            var weapons = keyValuePair.Key;

            var characterInitData =
                CharacterInitData.GenPlayerByConfig(1, 1, new[] {weapons}, size.small,
                    1, new Dictionary<passive_id, uint>());
#if DEBUG
            Console.Out.WriteLine($"Test P1 ok");
#endif
            return characterInitData;
        }

        public static CharacterInitData TestPlayer2()
        {
            var keyValuePair = CommonConfig.Configs.weapons.First();
            var weapons = keyValuePair.Key;

            var characterInitData =
                CharacterInitData.GenPlayerByConfig(2, 1, new[] {weapons}, size.small,
                    1, new Dictionary<passive_id, uint>());
#if DEBUG
            Console.Out.WriteLine($"Test P2 ok");
#endif
            return characterInitData;
        }

        public static MapInitData TestInitData()
        {
            var testMap = TestMap();
            var testSightMap = TestSightMap();
            var pt1 = new TwoDPoint(0f, 0.0f);
            var pt2 = new TwoDPoint(5f, 0.0f);
            var startPts = new StartPts(new[] {pt1});
            var startPts2 = new StartPts(new[] {pt2});

            var startPaces = new Dictionary<int, StartPts>
            {
                {1, startPts}, {2, startPts2}
            };
#if DEBUG
            Console.Out.WriteLine($"test map init ok");
#endif
            return new MapInitData(testSightMap, testMap, startPaces,
                new HashSet<ApplyDevice>(), testSightMap);
        }

        private static WalkMap TestMap()
        {
            var tuples = new List<(Poly, bool)>
            {
                (TestPoly(), false)
                // , (TestPoly2(), true)
                // , (TestPoly3(), true)
            };
#if DEBUG
            Console.Out.WriteLine("test_walk_map");
#endif
            var mapByPolys = WalkMap.CreateMapByPolys(tuples);
#if DEBUG
            Console.Out.WriteLine("test_walk_map_ok");
#endif
            return mapByPolys;
        }

        public static Poly TestPoly()
        {
            var pt1 = new TwoDPoint(10.0f, 10.0f);
            var pt2 = new TwoDPoint(10f, -10f);
            var pt3 = new TwoDPoint(-10.0f, -10f);
            var pt4 = new TwoDPoint(-10.0f, 10f);

            var twoDPoints = new[] {pt1, pt2, pt3, pt4};

            var poly = new Poly(twoDPoints);
            return poly;
        }

        public static Poly TestPoly2()
        {
            var pt1 = new TwoDPoint(2.0f, 10.0f);
            var pt2 = new TwoDPoint(2.0f, 2.0f);
            var pt3 = new TwoDPoint(-2.0f, 2.0f);
            var pt4 = new TwoDPoint(-2.0f, 10.0f);

            var twoDPoints = new[] {pt1, pt2, pt3, pt4};

            var poly = new Poly(twoDPoints);
            return poly;
        }

        public static Poly TestPoly3()
        {
            var pt1 = new TwoDPoint(2.0f, -10.0f);
            var pt2 = new TwoDPoint(2.0f, -2.0f);
            var pt3 = new TwoDPoint(-2.0f, -2.0f);
            var pt4 = new TwoDPoint(-2.0f, -10.0f);
            var twoDPoints = new[] {pt1, pt2, pt3, pt4};

            var poly = new Poly(twoDPoints);
            return poly;
        }

        public static SightMap TestSightMap()
        {
            var tuples = new List<(Poly, bool)>
            {
                (TestPoly(), false), (TestPoly2(), true), (TestPoly3(), true)
            };

            var mapByPolys = SightMap.GenByConfig(tuples, new TwoDVectorLine[] { });
            return mapByPolys;
        }
    }
}