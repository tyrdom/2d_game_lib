using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using collision_and_rigid;
using game_config;
using game_stuff;
using rogue_chapter_maker;

namespace rogue_game
{
    [Serializable]
    public class Chapter
    {
        public Dictionary<int, PveMap> MGidToMap { get; }
        public PveMap Entrance { get; }
        public PveMap Finish { get; }

        public Chapter(Dictionary<int, PveMap> mGidToMap, PveMap entrance, PveMap finish)
        {
            MGidToMap = mGidToMap;
            Entrance = entrance;
            Finish = finish;
        }

        public bool IsPass()
        {
            return Finish.IsClear;
        }

        public static Chapter GenById(int id)
        {
            throw new System.NotImplementedException();
        }

        public static int[] ChooseInts(MapType mapType, rogue_game_chapter gameChapter)
        {
            return mapType switch
            {
                MapType.BigStart => gameChapter.BigMapResRandIn,
                MapType.BigEnd => gameChapter.BigMapResRandIn,
                MapType.Small => gameChapter.BigMapResRandIn,
                MapType.Big => gameChapter.BigMapResRandIn,
                MapType.SmallStart => gameChapter.BigMapResRandIn,
                MapType.SmallEnd => gameChapter.BigMapResRandIn,
                MapType.Vendor => gameChapter.VendorMapResRandIn,
                MapType.Hangar => gameChapter.HangarMapResRandIn,
                _ => throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null)
            };
        }

        public static Chapter GenByConfig(rogue_game_chapter gameChapter, Random random)
        {
            var chapterMapTop = ChapterMapTop.GenAChapterTopByConfig(gameChapter);
            var dictionary = new Dictionary<PointMap, int>();
            var pointMaps = chapterMapTop.PointMaps.ToList();
            for (var i = 0; i < pointMaps.Count; i++)
            {
                dictionary[pointMaps[i]] = i;
            }

            var pveEmptyMaps = dictionary.ToDictionary(pair => pair.Value, pair =>
                {
                    var pointMap = pair.Key;
                    var value = pair.Value;
                    var chooseInts = ChooseInts(pointMap.MapType, gameChapter);
                    var next =
                        chooseInts.Any()
                            ? chooseInts.Length > 1
                                ? chooseInts[random.Next(chooseInts.Length)]
                                : chooseInts[0]
                            : throw new IndexOutOfRangeException();
                    return PveMap.GenEmptyPveMap(pointMap, next, value);
                }
            );
            var start = pointMaps.FirstOrDefault(x => x.MapType == MapType.BigStart || x.MapType == MapType.SmallStart);
            var end = pointMaps.FirstOrDefault(x => x.MapType == MapType.BigEnd || x.MapType == MapType.SmallEnd);

            PveMap Converter(PointMap? x) =>
                dictionary.TryGetValue(x ?? throw new InvalidOperationException(), out var value)
                    ? pveEmptyMaps.TryGetValue(value, out var map) ? map : throw new DirectoryNotFoundException()
                    : throw new DirectoryNotFoundException();

            var startMap = Converter(start);
            var endMap = Converter(end);

            static TwoDPoint Func(int id, direction direct) =>
                game_stuff.LocalConfig.PerLoadMapTransPort.TryGetValue(id, out var dd)
                    ? dd.TryGetValue(direct, out var twoDp)
                        ? twoDp.FirstOrDefault() ?? throw new IndexOutOfRangeException()
                        : throw new DirectoryNotFoundException()
                    : throw new DirectoryNotFoundException();


            // add teleport
            foreach (var pointMap in pointMaps)
            {
                var value = Converter(pointMap);

                var applyDevices = pointMap.Links.GroupBy(x => x.Side).Select(pointMapLink =>
                {
                    var linkTo = pointMapLink.FirstOrDefault()?.LinkTo ?? throw new IndexOutOfRangeException();
                    var direction = pointMapLink.Key;
                    var playGroundResMId = value.PlayGround.ResMId;


                    var fromPt = Func(playGroundResMId, direction);

                    var toPlayground = pveEmptyMaps[dictionary[linkTo]].PlayGround;
                    var groundResMId = toPlayground.ResMId;
                    var toPt = Func(groundResMId, direction.OppositeDirection());

                    var teleportUnit = new TeleportUnit(toPlayground, toPt, linkTo.MapType.AllowSizes());
                    var applyDevice = new ApplyDevice(teleportUnit, fromPt, false);
                    return applyDevice;
                });
                value.PlayGround.AddRangeMapInteractable(applyDevices);
            }


            throw new System.NotImplementedException();
        }
    }
}