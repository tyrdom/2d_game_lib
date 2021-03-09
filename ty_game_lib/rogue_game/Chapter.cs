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


        public static Chapter GenMapsById(int id, Random random)
        {
            return CommonConfig.Configs.rogue_game_chapters.TryGetValue(id, out var chapter)
                ? GenByConfig(chapter, random)
                : throw new KeyNotFoundException();
        }

        public static (int[] creep, int[] boss) ChooseNpcId(MapType mapType, rogue_game_chapter gameChapter,
            Random random)
        {
            var enumerable = gameChapter.CreepRandIn.ChooseRandCanSame(gameChapter.SmallCreepNum, random).Union(
                gameChapter.EliteRandomIn.ChooseRandCanSame(gameChapter.SmallEliteNum, random));
            var union = gameChapter.CreepRandIn.ChooseRandCanSame(gameChapter.BigCreepNum, random).Union(
                gameChapter.EliteRandomIn.ChooseRandCanSame(gameChapter.BigEliteNum, random));
            return mapType switch
            {
                MapType.BigStart => (new int[] { }, new int[] { }),
                MapType.BigEnd => (union.ToArray(),
                    gameChapter.BigBossCreepRandIn.ChooseRandCanSame(1, random).ToArray()),
                MapType.Small => (enumerable.ToArray(), new int[] { }),
                MapType.Big => (union.ToArray(), new int[] { }),
                MapType.SmallStart => (new int[] { }, new int[] { }),
                MapType.SmallEnd => (enumerable.ToArray(),
                    gameChapter.SmallBossCreepRandIn.ChooseRandCanSame(1, random).ToArray()),
                MapType.Vendor => (new int[] { }, new int[] { }),
                MapType.Hangar => (new int[] { }, new int[] { }),
                _ => throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null)
            };
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
#if DEBUG
            Console.Out.WriteLine($"{chapterMapTop}");
#endif
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
                    var next = chooseInts.ChooseRandCanSame(1, random).First();
                    var (creep, boss) = ChooseNpcId(pointMap.MapType, gameChapter, random);
                    return PveMap.GenEmptyPveMap(pointMap, next, value, creep, boss);
                }
            );
            var start = pointMaps.FirstOrDefault(x => x.MapType == MapType.BigStart || x.MapType == MapType.SmallStart);
            var end = pointMaps.FirstOrDefault(x => x.MapType == MapType.BigEnd || x.MapType == MapType.SmallEnd);

            PveMap Converter(PointMap? x) =>
                dictionary.TryGetValue(x ?? throw new InvalidOperationException(), out var value)
                    ? pveEmptyMaps.TryGetValue(value, out var map) ? map : throw new KeyNotFoundException()
                    : throw new KeyNotFoundException();

            var startMap = Converter(start);
            var endMap = Converter(end);

            static TwoDPoint Func(int id, direction direct) =>
                game_stuff.StuffLocalConfig.PerLoadMapTransPort.TryGetValue(id, out var dd)
                    ? dd.TryGetValue(direct, out var twoDp)
                        ? twoDp.FirstOrDefault() ?? throw new IndexOutOfRangeException()
                        : throw new KeyNotFoundException()
                    : throw new KeyNotFoundException();


            // add teleport
            foreach (var pointMap in pointMaps)
            {
                var value = Converter(pointMap);

                var applyDevices = pointMap.Links.GroupBy(x => x.Side).Select(pointMapLink =>
                {
                    var linkTo = pointMapLink.FirstOrDefault()?.LinkTo
                                 ?? throw new IndexOutOfRangeException(
                                     $"{pointMap.Links.Count} : direction {pointMapLink.Key} {pointMap.Slot.x}| {pointMap.Slot.y}");
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

            return new Chapter(pveEmptyMaps, startMap, endMap);
        }
    }
}