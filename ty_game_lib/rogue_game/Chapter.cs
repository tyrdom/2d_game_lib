using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using collision_and_rigid;
using game_config;
using game_stuff;
using rogue_chapter_maker;

namespace rogue_game
{
    public class Chapter
    {
        public Dictionary<PointMap, int> MapOnYSlotArray { get; }
        public Dictionary<int, PveMap> MGidToMap { get; }
        public PveMap Entrance { get; }
        public PveMap Finish { get; }
        public int ExtraPassiveNum { get; }

        public int ChapterId { get; }

        public InitChapter GenInitChapterPush()
        {
            return new InitChapter(GetReachedMapsMGidS());
        }

        public PushChapterGoNext GenNewChapterMap()
        {
            var genChapterTuples = GenChapterTuples(MapOnYSlotArray);
            var ySlotArray = ToYSlotArray(genChapterTuples);
            return new PushChapterGoNext(ySlotArray);
        }

        public ImmutableHashSet<IGameResp> GenLoadChapterMsg()
        {
            var genLoadChapterMsg = new IGameResp[] {GenNewChapterMap(), GenInitChapterPush()};
            return genLoadChapterMsg.ToImmutableHashSet();
        }

        public int[] GetReachedMapsMGidS()
        {
            var enumerable = MGidToMap.Where(x => x.Value.IsReached).Select(x => x.Key);
            return enumerable.ToArray();
        }

        public Chapter(Dictionary<int, PveMap> mGidToMap, PveMap entrance, PveMap finish, int extraPassiveNum,
            Dictionary<PointMap, int> mapOnYSlotArray, int chapterId)
        {
            MGidToMap = mGidToMap;
            Entrance = entrance;
            Finish = finish;
            ExtraPassiveNum = extraPassiveNum;
            MapOnYSlotArray = mapOnYSlotArray;
            ChapterId = chapterId;
        }

#if DEBUG
        public IGameResp SetPass(int callSeat, IGameRequest gameRequest)
        {
            Finish.ForceClear();
            return new QuestOkResult(callSeat, gameRequest);
        }
#endif
        public bool IsPass()
        {
            return Finish.IsClear;
        }


        public static (Chapter genByConfig, (int x, MapType MapType, int MGid)[][] ySlotArray) GenChapterById(int id,
            Random random)
        {
            return CommonConfig.Configs.rogue_game_chapters.TryGetValue(id, out var chapter)
                ? GenByConfig(chapter, random)
                : throw new KeyNotFoundException();
        }

        public static (int[] creep, int[] boss) ChooseNpcId(MapType mapType, rogue_game_chapter gameChapter,
            Random random)
        {
            var chooseRandCanSame = gameChapter.EliteRandomIn.Any()
                ? gameChapter.EliteRandomIn.ChooseRandCanSame(gameChapter.SmallEliteNum, random)
                : ImmutableArray<int>.Empty;
            var smallCreep = gameChapter.CreepRandIn.Any()
                ? gameChapter.CreepRandIn.ChooseRandCanSame(gameChapter.SmallCreepNum, random).ToList()
                : new List<int>();
            smallCreep.AddRange(chooseRandCanSame);

            var randCanSame = gameChapter.BigEliteRandomIn.Any()
                ? gameChapter.BigMapResRandIn.ChooseRandCanSame(gameChapter.BigEliteNum, random)
                : ImmutableArray<int>.Empty;
            var bigCreep = gameChapter.BigCreepRandIn.Any()
                ? gameChapter.BigCreepRandIn.ChooseRandCanSame(gameChapter.BigCreepNum, random).ToList()
                : new List<int>();
            bigCreep.AddRange(randCanSame);
#if DEBUG
            Console.Out.WriteLine(
                $"small creep{gameChapter.SmallCreepNum + gameChapter.SmallEliteNum} | big creep {gameChapter.BigCreepNum + gameChapter.BigEliteNum}");
            Console.Out.WriteLine($"so small {smallCreep.Count()}  big {bigCreep.Count()}");
#endif
            return mapType switch
            {
                MapType.BigStart => (new int[] { }, new int[] { }),
                MapType.BigEnd => (bigCreep.ToArray(),
                    gameChapter.BigBossCreepRandIn.ChooseRandCanSame(1, random).ToArray()),

                MapType.Small => (smallCreep.ToArray(), new int[] { }),
                MapType.Big => (bigCreep.ToArray(), new int[] { }),

                MapType.SmallStart => (new int[] { }, new int[] { }),
                MapType.SmallEnd => (smallCreep.ToArray(),
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
                MapType.BigStart => gameChapter.StartRandIn,
                MapType.BigEnd => gameChapter.BigMapResRandIn,
                MapType.Small => gameChapter.SmallMapResRandIn,
                MapType.Big => gameChapter.BigMapResRandIn,
                MapType.SmallStart => gameChapter.StartRandIn,
                MapType.SmallEnd => gameChapter.SmallMapResRandIn,
                MapType.Vendor => gameChapter.VendorMapResRandIn,
                MapType.Hangar => gameChapter.HangarMapResRandIn,
                _ => throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null)
            };
        }

        public static (MapType MapType, (int x, int y) Slot, int Value)[] GenChapterTuples(
            Dictionary<PointMap, int> charMapTop)
        {
            var valueTuples = charMapTop.Select(x => (x.Key.MapType, x.Key.Slot, x.Value)).ToArray();
            return valueTuples;
        }

        public static (int x, MapType MapType, int MGid)[][] ToYSlotArray(
            (MapType MapType, (int x, int y) Slot, int GId)[] valueTuples)
        {
            var maxX = valueTuples.Select(tuple => tuple.Slot.x).Max();
            var minX = valueTuples.Select(tuple => tuple.Slot.x).Min();
            var enumerableX = Enumerable.Range(minX, maxX - minX + 1);
            var list = valueTuples.GroupBy(t => t.Slot.y).ToList();
            list.Sort((x, y) => x.Key.CompareTo(y.Key));
            var enumerable = list.Select(x =>
                    x.Select(xx => (xx.Slot.x, xx.MapType, xx.GId))
                        .ToArray())
                ;
            return enumerable.ToArray();
        }

        public static (MapType mapType, int mgid)[][] YSlotsToMapTypeMatrix(
            (int x, MapType MapType, int GId)[][] list)
        {
            var selectMany = list.SelectMany(x => x.Select(xx => xx.x)).ToArray();
            var maxX = selectMany.Max();
            var minX = selectMany.Min();
            var enumerableX = Enumerable.Range(minX, maxX - minX + 1);


            var enumerable = list.Select(x =>
            {
                var dictionary = x.ToDictionary(p => p.x, p => (p.MapType, p.GId));

                return enumerableX.Select(xx => dictionary.TryGetValue(xx, out var mapType)
                        ? mapType
                        : (MapType.Nothing, -1))
                    .ToArray();
            }).ToArray();
            return enumerable;
        }

        public static Chapter GenBySave(Dictionary<PointMap, int> dictionary, rogue_game_chapter gameChapter,
            Random random, PlayGroundSaveData[] playGroundSaveDataS)
        {
            var playGroundSaveDatas = playGroundSaveDataS.ToDictionary(x => x.MGid, x => x);
            var pveEmptyMaps = dictionary.ToDictionary(pair => pair.Value, pair =>
                {
                    var pointMap = pair.Key;
                    var gMid = pair.Value;
                    var chooseInts = ChooseInts(pointMap.MapType, gameChapter);
                    var tryGetValue = playGroundSaveDatas.TryGetValue(gMid, out var save);
                    var resId = tryGetValue
                        ? save.ResId
                        : chooseInts.ChooseRandCanSame(1, random).First();
                    var (creep, boss) = ChooseNpcId(pointMap.MapType, gameChapter, random);
                    var selectMany =
                        tryGetValue
                            ? save.SaleUnitSaves.Select(x => x.LoadToSaleUnit())
                            : gameChapter.SaleUnits
                                .SelectMany(x =>
                                    x.Range.ChooseRandCanSame(x.Num, random)
                                        .Select(SaleUnitStandard.GenById)
                                );
                    return PveMap.GenEmptyPveMap(pointMap, resId, gMid, creep, boss, selectMany);
                }
            );
            var start = dictionary.Keys.FirstOrDefault(x =>
                x.MapType == MapType.BigStart || x.MapType == MapType.SmallStart);
            var end = dictionary.Keys.FirstOrDefault(x => x.MapType == MapType.BigEnd || x.MapType == MapType.SmallEnd);

            var startMap = Converter(start, dictionary, pveEmptyMaps);
            var endMap = Converter(end, dictionary, pveEmptyMaps);
            AddTeleports(dictionary, pveEmptyMaps);

            var genByConfig = new Chapter(pveEmptyMaps, startMap, endMap, gameChapter.ExtraPassiveNum, dictionary,
                gameChapter.id);
            return genByConfig;
        }

        public static (Chapter genByConfig, (int x, MapType MapType, int MGid)[][] ySlotArray) GenByConfig(
            rogue_game_chapter gameChapter, Random random)
        {
            var chapterMapTop = ChapterMapTop.GenAChapterTopByConfig(gameChapter);
#if DEBUG
            Console.Out.WriteLine($"{chapterMapTop}");
#endif
            var pointMaps = chapterMapTop.PointMaps.ToList();
            var dictionary = new Dictionary<PointMap, int>();

            for (var i = 0; i < pointMaps.Count; i++)
            {
                dictionary[pointMaps[i]] = i;
            }

            var genChapterTuples = GenChapterTuples(dictionary);
            var ySlotArray = ToYSlotArray(genChapterTuples);

            var pveEmptyMaps = dictionary.ToDictionary(pair => pair.Value, pair =>
                {
                    var pointMap = pair.Key;
                    var value = pair.Value;
                    var chooseInts = ChooseInts(pointMap.MapType, gameChapter);
                    var next = chooseInts.ChooseRandCanSame(1, random).First();
                    var (creep, boss) = ChooseNpcId(pointMap.MapType, gameChapter, random);
                    var selectMany = gameChapter.SaleUnits
                        .SelectMany(x =>
                            x.Range.ChooseRandCanSame(x.Num, random)
                                .Select(SaleUnitStandard.GenById)
                        );
#if DEBUG
                    Console.Out.WriteLine(
                        $"creep {creep.Aggregate("", (s, x) => s + x + ",")} and boss {boss.Aggregate("", (s, x) => s + x + ",")}");
#endif

                    return PveMap.GenEmptyPveMap(pointMap, next, value, creep, boss, selectMany);
                }
            );
            var start = dictionary.Keys.FirstOrDefault(x =>
                x.MapType == MapType.BigStart || x.MapType == MapType.SmallStart);
            var end = dictionary.Keys.FirstOrDefault(x => x.MapType == MapType.BigEnd || x.MapType == MapType.SmallEnd);

            var startMap = Converter(start, dictionary, pveEmptyMaps);
            var endMap = Converter(end, dictionary, pveEmptyMaps);


            // add teleport
            AddTeleports(dictionary, pveEmptyMaps);

            var genByConfig = new Chapter(pveEmptyMaps, startMap, endMap, gameChapter.ExtraPassiveNum, dictionary,
                gameChapter.id);
            return (genByConfig, ySlotArray);
        }

        private static void AddTeleports(Dictionary<PointMap, int> dictionary,
            IReadOnlyDictionary<int, PveMap> pveEmptyMaps)
        {
            foreach (var pointMap in dictionary.Keys)
            {
                var value = Converter(pointMap, dictionary, pveEmptyMaps);

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
        }

        private static TwoDPoint Func(int id, direction direct)
        {
            return game_stuff.StuffLocalConfig.PerLoadMapTransPort.TryGetValue(id, out var dd)
                ? dd.TryGetValue(direct, out var twoDp)
                    ? twoDp.FirstOrDefault() ?? throw new IndexOutOfRangeException()
                    : throw new KeyNotFoundException()
                : throw new KeyNotFoundException();
        }

        private static PveMap Converter(PointMap? x, IReadOnlyDictionary<PointMap, int> dictionary,
            IReadOnlyDictionary<int, PveMap> pveEmptyMaps)
        {
            return dictionary.TryGetValue(x ?? throw new InvalidOperationException(), out var value)
                ? pveEmptyMaps.TryGetValue(value, out var map) ? map : throw new KeyNotFoundException()
                : throw new KeyNotFoundException();
        }
    }
}