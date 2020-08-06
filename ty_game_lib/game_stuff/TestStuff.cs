using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public static class TestStuff
    {
        public static (PlayGround, Dictionary<int, HashSet<CharInitMsg>>) TestPlayGround()
        {
            var initPlayGround = PlayGround.InitPlayGround(new[] {TestPlayer1(), TestPlayer2()}, TestInitData());

            return initPlayGround;
        }

        private static PlayerInitData TestPlayer1()
        {
            var keyValuePair = TempConfig.Configs.weapons.First();
            var weapons = keyValuePair.Value;
            return PlayerInitData.GenByConfig(1, 1, new[] {weapons}, size.small, 6/10f, 1/10f, 0.05f);
        }

        private static PlayerInitData TestPlayer2()
        {
            var keyValuePair = TempConfig.Configs.weapons.First();
            var weapons = keyValuePair.Value;
            return PlayerInitData.GenByConfig(2, 2, new[] {weapons}, size.small, 6/10f, 1/10f, 0.05f);
        }

        private static MapInitData TestInitData()
        {
            var testMap = TestMap();
            var testSightMap = TestSightMap();
            var pt1 = new TwoDPoint(3.0f, 0.0f);
            var pt2 = new TwoDPoint(7f, 0.0f);
            var startPts = new StartPts(new List<TwoDPoint> {pt1});
            var startPts2 = new StartPts(new List<TwoDPoint> {pt2});

            var startPaces = new Dictionary<int, StartPts>
            {
                {1, startPts}, {2, startPts2}
            };
            return new MapInitData(testSightMap, testMap, startPaces);
        }

        private static WalkMap TestMap()
        {
            var tuples = new List<(Poly, bool)>
            {
                (TestPoly(), false)
            };

            var mapByPolys = WalkMap.CreateMapByPolys(tuples);
            return mapByPolys;
        }

        private static Poly TestPoly()
        {
            var pt1 = new TwoDPoint(0.0f, 0.0f);
            var pt2 = new TwoDPoint(10.0f, 10.0f);
            var pt3 = new TwoDPoint(20f, 0f);
            var pt4 = new TwoDPoint(30.0f, 10f);
            var pt5 = new TwoDPoint(40.0f, 0f);
            var pt6 = new TwoDPoint(20.0f, -20.0f);
            var twoDPoints = new[] {pt1, pt2, pt3, pt4, pt5, pt6};

            var poly = new Poly(twoDPoints);
            return poly;
        }

        public static SightMap TestSightMap()
        {
            var tuples = new List<(Poly, bool)>
            {
                (TestPoly(), false)
            };

            var mapByPolys = SightMap.GenByConfig(tuples, new TwoDVectorLine[] { });
            return mapByPolys;
        }
    }
}