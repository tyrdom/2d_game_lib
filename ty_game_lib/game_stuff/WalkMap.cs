using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class WalkMap
    {
        public Dictionary<size, WalkBlock> SizeToEdge { get; }

        private WalkMap(Dictionary<size, WalkBlock> sizeToEdge)
        {
            SizeToEdge = sizeToEdge;
        }


        public static WalkMap CreateMapByPolys(List<(Poly, bool)> lp, float fixRadMulti = 1f)
        {
            var sizeToR = CommonConfig.Configs.bodys;
            var walkBlocks = new Dictionary<size, WalkBlock>();
            foreach (var keyValuePair in sizeToR)
            {
                var rad = keyValuePair.Value.rad;
                var valueRad = rad * fixRadMulti;
#if DEBUG
                Console.Out.WriteLine(
                    $"CreateMap for {keyValuePair.Key} : rad {rad} multi{fixRadMulti} result {valueRad}");

#endif
                var genWalkBlockByPolys = SomeTools.GenWalkBlockByPolygons(lp, valueRad, 6);
                walkBlocks[keyValuePair.Key] = genWalkBlockByPolys;
            }

            return new WalkMap(walkBlocks);
        }
    }
}