using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class PlayGround
    {
        public int MgId { get; }
        public int ResMId { get; }
        private Zone MoveZone { get; }
        private Dictionary<int, (IQSpace playerBodies, IQSpace Traps)> TeamToBodies { get; } //角色实体放置到四叉树中，方便子弹碰撞逻辑
        private SightMap? SightMap { get; } //视野地图

        private SightMap? BulletBlockMap { get; } //子弹穿透阻挡，无特殊情况和视野地图相当
        public WalkMap WalkMap { get; } //碰撞地图

        private Dictionary<int, StartPts> Entrance { get; }
        private Dictionary<int, CharacterBody> GidToBody { get; } //gid到玩家地图实体对应
        private Dictionary<int, List<IHitMedia>> TeamToHitMedia { get; } // 碰撞媒体缓存，以后可能会有持续多帧的子弹

        private IQSpace MapInteractableThings { get; } // 互动物品，包括地上的武器，道具，被动技能，空载具，售卖机等

        private Dictionary<int, (ImmutableHashSet<INotMoveCanBeAndNeedSew>, ImmutableHashSet<CharacterBody>)>
            LastTickRecord { get; }


        private int NowMapInstanceInteractableId { get; set; }

        private List<(int gid, TelePortMsg)> TempTeleMsgToThisMap { get; }

        public List<ApplyDevice> GetMapApplyDevices()
        {
            var applyDevices = new List<ApplyDevice>();

            MapInteractableThings.ForeachBoxDoWithOutMove<List<ApplyDevice>, ApplyDevice>(
                (idp, aps) => { aps.Add(idp); }, applyDevices);
            return applyDevices;
        }

        private int GenAMapInstanceId()
        {
            NowMapInstanceInteractableId++;
            if (NowMapInstanceInteractableId >= int.MaxValue)
            {
                NowMapInstanceInteractableId = 0;
            }

            return NowMapInstanceInteractableId;
        }

        private int[] GenALotInstanceId(int count)
        {
            var ints = new List<int>();
            for (var i = 0; i < count; i++)
            {
                var mapApplyDevices = GenAMapInstanceId();
                ints.Add(mapApplyDevices);
            }

            return ints.ToArray();
        }

        private PlayGround(Dictionary<int, (IQSpace playerBodies, IQSpace Traps)> teamToBodies, SightMap? sightMap,
            WalkMap walkMap,
            Dictionary<int, CharacterBody> gidToBody, IQSpace mapInteractableThings, int mgId,
            int resMId, Zone moveZone, Dictionary<int, StartPts> entrance, SightMap? bulletBlockMap)
        {
            TeamToBodies = teamToBodies;
            SightMap = sightMap;
            WalkMap = walkMap;
            GidToBody = gidToBody;
            MapInteractableThings = mapInteractableThings;
            MgId = mgId;
            ResMId = resMId;
            MoveZone = moveZone;
            Entrance = entrance;
            BulletBlockMap = bulletBlockMap;
            TeamToHitMedia = new Dictionary<int, List<IHitMedia>>();
            LastTickRecord =
                new Dictionary<int, (ImmutableHashSet<INotMoveCanBeAndNeedSew>, ImmutableHashSet<CharacterBody>)>();
            NowMapInstanceInteractableId = 0;
            TempTeleMsgToThisMap = new List<(int gid, TelePortMsg)>();
        }

        public static PlayGround GenEmptyPlayGround(int resId, int genId)
        {
            if (StuffLocalConfig.PerLoadMapConfig.TryGetValue(resId, out var initData))
            {
                return GenEmptyPlayGround(initData, genId, resId);
            }

            throw new Exception($"no good resId {resId}");
        }

        //初始化状态信息,包括玩家信息和地图信息
        private static PlayGround GenEmptyPlayGround(MapInitData initData, int genId, int resId)
        {
            var playGround = InitPlayGround(new CharacterInitData[] { }, initData, genId, resId);
            return playGround;
        }

        public static MapInitData GenEmptyByConfig(map_raws mapRaws)
        {
            var mapRawsWalkRawMap = mapRaws.WalkRawMap;
            var mapRawsSightRawMap = mapRaws.SightRawMap;
            var mapRawsBulletRawMap = mapRaws.BulletRawMap;
            var enumerable = mapRawsWalkRawMap.Select(x => x.GenPoly()).ToArray();
            var walkMap = enumerable.Any()
                ? enumerable.PloyListCheckOk() ? WalkMap.CreateMapByPolys(enumerable.PloyListMark()) :
                throw new Exception($"no good walk raw poly in {mapRaws.id} ")
                : throw new Exception("must have walk map");
            var array = mapRawsSightRawMap.Select(x => x.GenPoly()).ToArray();
            var sightMap = array.Any()
                ? array.PloyListCheckOk() ? SightMap.GenByConfig(array.PloyListMark()) :
                throw new Exception($"no good sight raw poly in {mapRaws.id} ")
                : null;
            var polys = mapRawsBulletRawMap.Select(x => x.GenPoly()).ToArray();
            var genByConfig = polys.Any()
                ? polys.PloyListCheckOk() ? SightMap.GenByConfig(polys.PloyListMark()) :
                throw new Exception($"no good bullet raw poly in {mapRaws.id} ")
                : null;

            var mapRawsStartPoints = mapRaws.StartPoints;
            var startPointsLength = mapRawsStartPoints.Length;
            var pointsMap = Enumerable.Range(0, startPointsLength).Select(pp => (pp, mapRawsStartPoints[pp]))
                .ToDictionary(tuple => tuple.pp,
                    tuple => new StartPts(tuple.Item2.Select(x => new TwoDPoint(x.x, x.y)).ToArray()));
            return new MapInitData(sightMap, walkMap, pointsMap, new HashSet<ApplyDevice>(), genByConfig);
        }

        public static PlayGround InitPlayGround(
            IEnumerable<CharacterInitData> playerInitData, MapInitData mapInitData, int genMapId, int mapResId)
        {
            var bodies = new Dictionary<int, HashSet<CharacterBody>>();
            var characterBodies = new Dictionary<int, CharacterBody>();

            foreach (var initData in playerInitData)
            {
                var initDataTeamId = initData.TeamId;
                if (!mapInitData.TeamToStartPt.TryGetValue(initDataTeamId, out var startPts))
                {
                    throw new KeyNotFoundException($"not have such team start{initDataTeamId}");
                }

                var twoDPoint = startPts.GenPt();
                var genCharacterBody = initData.GenCharacterBody(twoDPoint);
                if (bodies.TryGetValue(initDataTeamId, out var playersToAdd))
                {
                    playersToAdd.Add(genCharacterBody);
                }
                else
                {
                    bodies[initDataTeamId] = new HashSet<CharacterBody> {genCharacterBody};
                }

                characterBodies[initData.Gid] = genCharacterBody;
            }

            var zone = mapInitData.GetZone();

            var emptyRootBranch = SomeTools.CreateEmptyRootBranch(zone);
            var emptyRootBranch2 = SomeTools.CreateEmptyRootBranch(zone);
            var spaces = bodies.ToDictionary(p => p.Key,
                p =>
                {
                    var hashSet = p.Value;
                    var aabbBoxShapes =
                        hashSet.Select(x => x.IdPointBox).OfType<IAaBbBox>().IeToHashSet();
                    emptyRootBranch.AddRangeAabbBoxes(aabbBoxShapes, CommonConfig.OtherConfig.qspace_max_per_level);
                    return (emptyRootBranch, emptyRootBranch2);
                });
            var emptyRootBranch3 = SomeTools.CreateEmptyRootBranch(zone);

#if DEBUG
            foreach (var qSpace in spaces)
            {
                Console.Out.WriteLine($"init:{qSpace.Key} team : {qSpace.Value.Item1.Count()} body");
            }
#endif

            var playGround = new PlayGround(spaces, mapInitData.SightMap, mapInitData.WalkMap, characterBodies,
                emptyRootBranch3, genMapId, mapResId, zone, mapInitData.TeamToStartPt, mapInitData.BulletBlockMap);

            playGround.AddRangeMapInteractable(mapInitData.StandardMapInteractableList);


            return playGround;
        }

        // 初始化状态消息输出
        private Dictionary<int, HashSet<CharInitMsg>> GenInitMsg()
        {
            var dictionary = new Dictionary<int, HashSet<CharInitMsg>>();
            var charInitMsgS =
                GidToBody.ToDictionary(p => p.Key,
                    p => p.Value.GenInitMsg());
            foreach (var kv in charInitMsgS)
            {
                if (dictionary.TryGetValue(kv.Key, out var initMsgList))
                {
                    initMsgList.Add(kv.Value);
                }
                else
                {
                    dictionary[kv.Key] = new HashSet<CharInitMsg> {kv.Value};
                }
            }

            return dictionary;
        }

        private Dictionary<int, Dictionary<int, Operate>> SepOperatesToTeam(Dictionary<int, Operate> gidToOperates)
        {
            var dictionary = new Dictionary<int, Dictionary<int, Operate>>();

            foreach (var kv in gidToOperates)
            {
                if (!GidToBody.TryGetValue(kv.Key, out var characterBody)) continue;
                if (dictionary.TryGetValue(characterBody.Team, out var gidToOp)
                )
                {
                    gidToOp[kv.Key] = kv.Value;
                }
                else
                {
                    dictionary[characterBody.Team] = new Dictionary<int, Operate> {{kv.Key, kv.Value}};
                }
            }

            return dictionary;
        }


        public PlayGroundGoTickResult
            PlayGroundGoATick(
                Dictionary<int, Operate> gidToOperates)
        {
            var actOut = new Dictionary<int, IEnumerable<IToOutPutResult>>();
            var playerBeHit = new Dictionary<int, HashSet<IRelationMsg>>();
            var trapBeHit = new Dictionary<int, Dictionary<int, HashSet<IRelationMsg>>>();
            var teamRadarSee = new Dictionary<int, HashSet<IPerceivable>>();
            var newBulletCollector = new HashSet<Bullet>();

            var everyBodyGoATick = EveryTeamGoATick(gidToOperates, actOut, newBulletCollector);

            MapInteractableGoATick(newBulletCollector);
            HitMediasDo(playerBeHit, trapBeHit, teamRadarSee);

            foreach (var twoDTwoP in everyBodyGoATick)
            {
                if (GidToBody.TryGetValue(twoDTwoP.Key, out var characterBody))
                {
                    characterBody.Move(twoDTwoP.Value);
                }
            }

            BodiesQSpaceReplace(playerBeHit);

            var playerSee = GetPlayerSense(newBulletCollector);

            var playerPerceivable =
                playerSee.ToImmutableDictionary(pair => pair.Key,
                    pair =>
                    {
                        var gid = pair.Key;
                        var (characterBodies, enumerable, hear) = pair.Value;
                        if (GidToBody.TryGetValue(gid, out var cb) &&
                            teamRadarSee.TryGetValue(cb.Team, out var sees))
                            return (characterBodies.Union(sees).ToImmutableHashSet(),
                                enumerable.ToImmutableHashSet(), hear.ToImmutableHashSet());
                        return (characterBodies.ToImmutableHashSet(),
                            enumerable.ToImmutableHashSet(), hear.ToImmutableHashSet());
                    });

            var playerTickSees = playerPerceivable.ToImmutableDictionary(x => x.Key,
                x => PlayerTickSense.GenPlayerSense(x.Value,
                    LastTickRecord.TryGetValue(x.Key, out var t)
                        ? t
                        : (ImmutableHashSet<INotMoveCanBeAndNeedSew>.Empty, ImmutableHashSet<CharacterBody>.Empty)));

            foreach (var keyValuePair in playerPerceivable)
            {
                var key = keyValuePair.Key;
                var (immutableHashSet, _, _) = keyValuePair.Value;
                var notMoveCanBeSews = immutableHashSet.OfType<INotMoveCanBeAndNeedSew>();
                var characterBodies = immutableHashSet.OfType<CharacterBody>();
                LastTickRecord[key] = (notMoveCanBeSews.ToImmutableHashSet(), characterBodies.ToImmutableHashSet());
            }

            if (TempTeleMsgToThisMap.Any())
            {
                foreach (var tuple in TempTeleMsgToThisMap)
                {
                    var (gid, telePortMsg) = tuple;
                    var toOutPutResults = actOut.TryGetValue(gid, out var acToOutPutResults)
                        ? acToOutPutResults.Append(telePortMsg)
                        : new IToOutPutResult[] {telePortMsg};
                    actOut[gid] = toOutPutResults;
                }

                TempTeleMsgToThisMap.Clear();
            }


            var valueTuple =
                new PlayGroundGoTickResult(
                    playerBeHit.ToImmutableDictionary(p => p.Key,
                        p => p.Value.ToImmutableHashSet()), trapBeHit.ToImmutableDictionary(pa => pa.Key,
                        pa => pa.Value.ToImmutableDictionary(pp => pp.Key,
                            pp => pp.Value.ToImmutableHashSet())),
                    playerTickSees, actOut.ToImmutableDictionary(p => p.Key, p => p.Value.ToImmutableArray()));

            return valueTuple;
        }

        private void MapInteractableGoATick(HashSet<Bullet> newBulletCollector)
        {
            var mapInteractableS = new List<IHitMedia>();

            var vehicleCanIns = new List<VehicleCanIn>();

            var tuples = new List<(TwoDPoint pos, Weapon[] weapons)>();

            MapInteractableThings
                .ForeachBoxDoWithOutMove<(List<IHitMedia> bullets,
                    List<VehicleCanIn> vehicleCanIns,
                    List<(TwoDPoint pos, Weapon[] weapons)> weaponsDrop
                    ), VehicleCanIn>(Act,
                    (mapInteractableS, vehicleCanIns, tuples));
            TeamToHitMedia[-1] = mapInteractableS;

            newBulletCollector.UnionWith(mapInteractableS.OfType<Bullet>());
            foreach (var vehicleCanIn in vehicleCanIns)
            {
                MapInteractableThings.RemoveSingleAaBbBox(vehicleCanIn);
            }

            var selectMany = tuples.SelectMany(x => x.weapons.Select(w => w.DropAsIMapInteractable(x.pos)));
            AddRangeMapInteractable(selectMany);
        }

        private static void Act(VehicleCanIn vehicleCanIn,
            (List<IHitMedia> bullets, List<VehicleCanIn> vehicleCanIns, List<(TwoDPoint pos, Weapon[] weapons)>
                weaponsDrop) valueTuple)
        {
            var (bullet, weapons) = vehicleCanIn.GoATick();

            if (bullet == null) return;
            var (bullets, vehicleCanIns, weaponsDrop) = valueTuple;
            bullets.Add(bullet);
            vehicleCanIns.Add(vehicleCanIn);
            var twoDPoint = vehicleCanIn.GetAnchor();
            weaponsDrop.Add((twoDPoint, weapons));
        }


        private Dictionary<int, (HashSet<IPerceivable> characterBodies, HashSet<Bullet> bulletSaw, HashSet<Bullet>
            bulletHear)> GetPlayerSense(
            ISet<Bullet> newBulletCollector)
        {
            var gidToCharacterBodies =
                new Dictionary<int, (HashSet<IPerceivable> characterBodies, HashSet<Bullet> bulletSaw, HashSet<Bullet>
                    bulletHear)>();
            foreach (var kv in GidToBody)
            {
                var key = kv.Key;
                var characterBody = kv.Value;
                var sightZone = characterBody.GetSightZone();
#if DEBUG
                // Console.Out.WriteLine($"gid::{characterBody.GetId()}");
#endif
                var characterBodies = new HashSet<IPerceivable>();
                var filterToBoxList = MapInteractableThings.FilterToBoxList<ICanBeAndNeedSaw, CharacterBody>(
                    (idp, acb) => acb.InSight(idp, SightMap),
                    characterBody, sightZone);
                characterBodies.UnionWith(filterToBoxList);
                foreach (var kv2 in TeamToBodies)
                {
                    var bTeam = kv2.Key;
                    var qSpace = kv2.Value.playerBodies;
                    var traps = kv2.Value.Traps;
// #if DEBUG
//                     Console.Out.WriteLine($"team{bTeam}:have:{qSpace.Count()} body");
// #endif
                    if (characterBody.Team == bTeam)
                    {
                        var filterToGIdPsList =
                            qSpace.FilterToBoxList<IdPointBox, bool>((x, y) => true, true).Select(x => x.IdPointShape)
                                .OfType<ICanBeAndNeedSaw>();

                        var ofType = traps.FilterToBoxList<IdPointBox, bool>((x, y) => true, true)
                            .Select(x => x.IdPointShape)
                            .OfType<ICanBeAndNeedSaw>();
                        // #if DEBUG
//                         Console.Out.WriteLine($"list::{filterToGIdPsList.ToArray().Length}");
// #endif                
                        characterBodies.UnionWith(filterToGIdPsList);
                        characterBodies.UnionWith(ofType);
                    }
                    else
                    {
                        var filterToGIdPsList =
                            qSpace.FilterToBoxList<IdPointBox, CharacterBody>((idp, acb) => acb.InSight(idp, SightMap),
                                characterBody, sightZone);

                        var bodies = filterToGIdPsList.Select(x => x.IdPointShape).OfType<CharacterBody>();
                        var trapsSee =
                            traps.FilterToBoxList<IdPointBox, CharacterBody>((idp, acb) => acb.InSight(idp, SightMap),
                                    characterBody, sightZone)
                                .Select(x => x.IdPointShape).OfType<Trap>()
                                .Where(t => t.CanBeSee);
#if DEBUG
                        Console.Out.WriteLine(
                            $" {characterBody.GetId()} : pos {characterBody.GetAnchor()}in t {characterBody.Team}:" +
                            $"look other team:{bTeam}::see {bodies.Count()}:in:{qSpace.Count()} all {GidToBody.Count()} now sight rad {characterBody.Sight.NowR}");
                        Console.Out.WriteLine(
                            $"look other team trap:{bTeam}::see {trapsSee.Count()}:in:{traps.Count()} all {GidToBody.Count()} now sight rad {characterBody.Sight.NowR}");
#endif

                        characterBodies.UnionWith(bodies);
                        characterBodies.UnionWith(trapsSee);
                    }
                }

                var bulletSaw = newBulletCollector.Where(x => characterBody.InSight(x, SightMap)).IeToHashSet();
                var bulletHear = newBulletCollector.Except(bulletSaw).Where(x => characterBody.Hear(x, BulletBlockMap))
                    .IeToHashSet();
                gidToCharacterBodies[key] = (characterBodies, bulletSaw, bulletHear);
            }

            return gidToCharacterBodies;
        }

        private void BodiesQSpaceReplace(IDictionary<int, HashSet<IRelationMsg>> playerBeHit)
        {
            foreach (var (playerBodies, _) in TeamToBodies.Select(kv => kv.Value))
            {
                var mapToDicGidToSth = playerBodies.MapToDicGidToSth(
                    (idPts, dic) =>
                    {
                        switch (idPts)
                        {
                            case CharacterBody characterBody:
                                var characterBodyBodySize = characterBody.GetSize();
                                if (dic.SizeToEdge.TryGetValue(characterBodyBodySize, out var walkBlock))
                                {
                                    return characterBody.RelocateWithBlock(walkBlock);
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(idPts));
                        }

                        throw new Exception("not good idPts");
                    }, WalkMap);

                var dd = mapToDicGidToSth.Where(x => x.Value.HasValue)
                    .Select(x => x.Key).ToArray();

                var buffDmgMsgs = mapToDicGidToSth.ToDictionary(
                    x => x.Key, x => x.Value!.Value.buffDmgMsg);
                foreach (var keyValuePair in buffDmgMsgs)
                {
                    if (keyValuePair.Value.HasValue && playerBeHit.TryGetValue(keyValuePair.Key, out var hashSet))
                    {
                        hashSet.Add(keyValuePair.Value);
                    }
                }
#if DEBUG
                // Console.Out.WriteLine($"team~ bf {TeamToBodies[1].playerBodies.Count()}");
#endif
                playerBodies.ReLocateIdBoxInQuadTree(dd, CommonConfig.OtherConfig.qspace_max_per_level);
#if DEBUG
                // Console.Out.WriteLine($"team~ af {TeamToBodies[1].playerBodies.Count()}");
#endif
            }
        }

        private static void RecordHitResult(IRelationMsg relationMsg,
            IDictionary<int, HashSet<IRelationMsg>> gidD, IDictionary<int, Dictionary<int, HashSet<IRelationMsg>>> tidD,
            IDictionary<int, HashSet<IPerceivable>> teamRadarSee)
        {
            if (relationMsg is RadarHit)
            {
                var id = relationMsg.CasterOrOwner.CharacterBody.Team;
                var twoDPoint = relationMsg.WhoTake.GetAnchor();
                var bodySize = relationMsg.WhoTake.GetSize();

                var radarSee = new RadarSee(twoDPoint, bodySize);
                if (teamRadarSee.TryGetValue(id, out var perceivableThings))
                {
                    perceivableThings.Add(radarSee);
                }
                else
                {
                    teamRadarSee[id] = new HashSet<IPerceivable> {radarSee};
                }

                return;
            }

            switch (relationMsg.WhoTake)
            {
                case CharacterBody characterBody:
                    var key = characterBody.GetId();
                    if (gidD.TryGetValue(key, out var hBullets))
                    {
                        hBullets.Add(relationMsg);
                    }
                    else
                    {
                        gidD[key] = new HashSet<IRelationMsg> {relationMsg};
                    }

                    break;
                case Trap trap:
                    var gid = trap.GetFinalCaster().GetId();
                    var tid = trap.GetId();
                    if (tidD.TryGetValue(gid, out var hBullets2))
                    {
                        if (hBullets2.TryGetValue(tid, out hBullets))
                        {
                            hBullets.Add(relationMsg);
                        }
                        else
                        {
                            hBullets2[tid] = new HashSet<IRelationMsg> {relationMsg};
                        }
                    }
                    else
                    {
                        var dictionary = new Dictionary<int, HashSet<IRelationMsg>>
                        {
                            [tid] = new HashSet<IRelationMsg> {relationMsg}
                        };
                        tidD[tid] = dictionary;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HitMediasDo(IDictionary<int, HashSet<IRelationMsg>> gidHitBy,
            IDictionary<int, Dictionary<int, HashSet<IRelationMsg>>> tidHitBy,
            IDictionary<int, HashSet<IPerceivable>> teamRadarSee)
        {
            foreach (var ii in TeamToHitMedia)
            {
                var team = ii.Key;
                var hitMedias = ii.Value;
                foreach (var hitMedia in hitMedias)
                {
                    switch (hitMedia.TargetType)
                    {
                        case ObjType.OtherTeam:
                            var selectMany = TeamToBodies.Where(pair => pair.Key != team)
                                .SelectMany(x => hitMedia
                                    .HitTeam(new[] {x.Value.playerBodies, x.Value.Traps}, BulletBlockMap)
                                );

                            foreach (var hr in selectMany)
                            {
                                RecordHitResult(hr, gidHitBy, tidHitBy, teamRadarSee);
                            }

                            break;
                        case ObjType.SameTeam:
                            if (TeamToBodies.TryGetValue(team, out var qSpace))
                            {
                                var enumerable = hitMedia.HitTeam(new[] {qSpace.playerBodies, qSpace.Traps},
                                        BulletBlockMap)
                                    ;
                                foreach (var hitResult in enumerable)
                                {
                                    RecordHitResult(hitResult, gidHitBy, tidHitBy, teamRadarSee);
                                }
                            }

                            break;
                        case ObjType.AllTeam:
                            var results = TeamToBodies.Values.SelectMany(x =>
                                hitMedia.HitTeam(new[] {x.playerBodies, x.Traps}, BulletBlockMap));

                            foreach (var hitResult in results)
                            {
                                RecordHitResult(hitResult, gidHitBy, tidHitBy, teamRadarSee);
                            }

                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                hitMedias.RemoveAll(x => !x.CanGoNextTick());
            }
        }

        private static readonly Func<IAaBbBox, bool> IsD = x => x is ApplyDevice;


        private static readonly Func<IAaBbBox, bool> IsC = x => x is CageCanPick;


        private static readonly Func<IAaBbBox, bool> IsV = x => x is VehicleCanIn;

        private Dictionary<int, ITwoDTwoP> EveryTeamGoATick(Dictionary<int, Operate> gidToOperates,
            IDictionary<int, IEnumerable<IToOutPutResult>> dictionary, HashSet<Bullet> newBulletCollector)
        {
            var sepOperatesToTeam = SepOperatesToTeam(gidToOperates);

            var twoDTwoPs = new Dictionary<int, ITwoDTwoP>();
            foreach (var tq in TeamToBodies)
            {
                var team = tq.Key;
                var (playerBodies, traps) = tq.Value;
                var gto = sepOperatesToTeam.TryGetValue(team, out var gidToOp)
                    ? gidToOp
                    : new Dictionary<int, Operate>();

                var mapToDicGidToSth = playerBodies.MapToDicGidToSth(GameTools.BodyGoATick, gto);
                var trapGoTickResults = traps.MapToIDict(GameTools.TrapGoATick);

                var thisTeamToRemove = new HashSet<IdPointBox>();

                var idPointBoxesToAdd = new HashSet<IdPointBox>();
                foreach (var trapGoTickResult in trapGoTickResults)
                {
                    var goTickResult = trapGoTickResult.Value;

                    var stillAlive = goTickResult.StillAlive;
#if DEBUG
                    // Console.Out.WriteLine($"trap go tick {trapGoTickResult.Key} :{trapGoTickResult.Value.StillAlive}");
#endif
                    if (!stillAlive)
                    {
                        if (goTickResult.Self?.IdPointBox != null)
                        {
                            thisTeamToRemove.Add(goTickResult.Self.IdPointBox);
#if DEBUG
                            Console.Out.WriteLine($"trap to remove {trapGoTickResult.Key} {thisTeamToRemove.Count}");
#endif
                        }

                        continue;
                    }

                    var launchBullet = goTickResult.LaunchBullet;

                    switch (launchBullet)
                    {
                        case null:
                            continue;
                        case Bullet bullet:
                            newBulletCollector.Add(bullet);
                            break;
                    }

                    var addBulletToDict = AddBulletToDict(team, launchBullet);
                    if (addBulletToDict != null) idPointBoxesToAdd.Add(addBulletToDict);
                }

                // foreach (var idPointBox in thisTeamToRemove)
                // {
                //     traps.RemoveSingleAaBbBox(idPointBox);
                // }
                traps.RemoveIdPointBoxes(thisTeamToRemove);

                foreach (var gtb in mapToDicGidToSth)
                {
                    // (gid, (twoDTwoP, bullet))

                    var gid = gtb.Key;
                    var aCharGoTickMsg = gtb.Value;
                    var twoDTwoP = aCharGoTickMsg.Move;
                    var stillAlive = aCharGoTickMsg.StillActive;
                    var actResults = aCharGoTickMsg.ActResults;

                    if (!stillAlive)
                    {
                        continue;
                    }


                    if (twoDTwoP != null)
                    {
#if DEBUG
                        // Console.Out.WriteLine(
                        //     $" {twoDTwoP.GetType().TypeHandle.Value.ToString()} :: move res :: {twoDTwoP.ToString()}");
#endif
                        twoDTwoPs[gid] = twoDTwoP;
                    }

                    var interactive = aCharGoTickMsg.GetThing;

                    if (interactive != null)
                    {
                        var removeSingleAaBbBox = MapInteractableThings.RemoveSingleAaBbBox(interactive);
                        if (!removeSingleAaBbBox)
                        {
                            throw new Exception("not such a interactable");
                        }
                    }

                    if (actResults.Any())
                    {
                        dictionary[gid] = actResults.OfType<IToOutPutResult>();
                        var mapInteractive = actResults.OfType<DropThings>().SelectMany(x => x.DropSet);
                        AddRangeMapInteractable(mapInteractive);
                    }


                    var interactCaller = aCharGoTickMsg.WhoInteractCall;
                    if (interactCaller != null)
                    {
                        var twoDPoint = interactCaller.GetAnchor();
                        (IAaBbBox? singleBox, bool needContinueCall) = aCharGoTickMsg.MapInteractive switch
                        {
                            MapInteract.RecycleCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                twoDPoint, IsC), true),
                            MapInteract.InVehicleCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                twoDPoint, IsV), false),
                            MapInteract.PickCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                twoDPoint, IsC), false),
                            MapInteract.KickVehicleCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                twoDPoint, IsV), true),
                            MapInteract.GetInfoCall => (
                                MapInteractableThings.InteractiveFirstSingleBox(twoDPoint, IsD),
                                false),
                            MapInteract.BuyOrApplyCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                twoDPoint, IsD), true),
                            null => throw new ArgumentOutOfRangeException(nameof(aCharGoTickMsg)),
                            _ => throw new ArgumentOutOfRangeException(nameof(aCharGoTickMsg))
                        };

                        switch (singleBox)
                        {
                            case null:
                                interactCaller.CharacterStatus.ResetCastAct();
                                break;
                            case IMapInteractable mapInteractable:
                                if (needContinueCall)
                                {
                                    mapInteractable.StartActTwoBySomeBody(interactCaller);
                                }
                                else
                                {
                                    mapInteractable.StartActOneBySomeBody(interactCaller);
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(singleBox));
                        }
                    }

                    var posMedia = aCharGoTickMsg.LaunchBullet;

                    var ofType = posMedia.OfType<Bullet>();
                    newBulletCollector.UnionWith(ofType);

                    var idPointBoxes = posMedia.Select(x => AddBulletToDict(team, x)).Where(x => x != null)
                        .OfType<IdPointBox>().ToArray();
                    // var addBulletToDict = AddBulletToDict(team, posMedia);
                    if (idPointBoxes.Any()) idPointBoxesToAdd.UnionWith(idPointBoxes);
                }

                traps.AddRangeAabbBoxes(idPointBoxesToAdd.OfType<IAaBbBox>().IeToHashSet(),
                    CommonConfig.OtherConfig.qspace_max_per_level);
            }

            return twoDTwoPs;
        }

        private IdPointBox? AddBulletToDict(int team, IPosMedia bullet)
        {
            switch (bullet)
            {
                case IHitMedia hitAbleMedia:

                    if (TeamToHitMedia.TryGetValue(team, out var b))
                    {
                        b.Add(hitAbleMedia);
                    }
                    else
                    {
                        TeamToHitMedia[team] = new List<IHitMedia> {hitAbleMedia};
                    }

                    return null;
                case Summon summons:
                    var genAMapInstanceId = GenAMapInstanceId();
                    var aTrap = summons.SetATrap(genAMapInstanceId);
                    return aTrap;

                default:
                    throw new ArgumentOutOfRangeException(nameof(bullet));
            }
        }

        public void AddSaleUnitsToMap(IEnumerable<ISaleUnit> interactableSet, int teamStartId)
        {
            if (!Entrance.TryGetValue(teamStartId, out var startPts))
                throw new DirectoryNotFoundException($"not such start id {teamStartId}");
            var enumerable = interactableSet.ToArray();
            var min = Math.Min(startPts.GetPtNum(), enumerable.Count());
            var twoDPoints = startPts.GenPt(min);
            var saleUnits = enumerable.Take(min);
            var applyDevices = twoDPoints.Zip(saleUnits, (x, y) => new ApplyDevice(y, x, false));
            AddRangeMapInteractable(applyDevices);
        }

        public void AddRangeMapInteractable(IEnumerable<IMapInteractable> interactableSet)
        {
            var mapInteractableS = interactableSet.IeToHashSet();

            foreach (var applyDevice in mapInteractableS.OfType<ApplyDevice>())
            {
                applyDevice.SetInPlayGround(this);
            }

            foreach (var notMoveCanBeSew in mapInteractableS)
            {
                notMoveCanBeSew.MapMarkId = GenAMapInstanceId();
            }

            MapInteractableThings.AddRangeAabbBoxes(mapInteractableS.OfType<IAaBbBox>().IeToHashSet(),
                CommonConfig.OtherConfig.qspace_max_per_level);
        }


        public void AddMapInteractable(IMapInteractable interactable)
        {
            switch (interactable)
            {
                case ApplyDevice applyDevice:
                    applyDevice.SetInPlayGround(this);
                    break;
            }

            interactable.MapMarkId = GenAMapInstanceId();

            MapInteractableThings.AddSingleAaBbBox(interactable, CommonConfig.OtherConfig.qspace_max_per_level);
        }

        public IEnumerable<int> RemoveBodies(HashSet<int> ints)
        {
            var enumerable = ints.Where(x => GidToBody.ContainsKey(x)).ToArray();

            var except = ints.Except(enumerable);
            var characterBodies = enumerable.Select(x => GidToBody[x]);
            var removeInQSpace = RemoveInQSpace(characterBodies);
            var ints1 = removeInQSpace.Select(x => x.GetId());
            var union = ints1.Union(except);
            return union;
        }

        private IEnumerable<IdPointBox> RemoveInQSpace(IEnumerable<CharacterBody> characterBodies)
        {
            var groupBy = characterBodies.GroupBy(x => x.Team);
            var selectMany = groupBy.SelectMany(bodies =>
            {
                if (!TeamToBodies.TryGetValue(bodies.Key, out var q)) return Enumerable.Empty<IdPointBox>();
                var idPointBoxes = bodies.Select(x => x.IdPointBox);
                return q.playerBodies.RemoveIdPointBoxes(idPointBoxes);
            });
            return selectMany;
        }


        public IEnumerable<CharacterBody> RemoveBodies(HashSet<CharacterBody> characterBodies)
        {
            if (!characterBodies.Any()) return characterBodies;
            IEnumerable<CharacterBody> enumerable = characterBodies.Where(x => GidToBody.Remove(x.GetId())).ToArray();
            var except = characterBodies.Except(enumerable);
            var selectMany = RemoveInQSpace(enumerable);
            var removeBodies = selectMany.Select(box => box.IdPointShape).OfType<CharacterBody>();

            var union = removeBodies.Union(except);
            return union;
        }

        public bool RemoveBody(int gid)
        {
            if (GidToBody.TryGetValue(gid, out var gidCharacterBody) && TeamToBodies.TryGetValue(gidCharacterBody.Team,
                out var valueTuple)
            )
            {
                return GidToBody.Remove(gid) &&
                       valueTuple.playerBodies.RemoveSingleAaBbBox(gidCharacterBody.IdPointBox);
            }

            return false;
        }

        public bool RemoveBody(CharacterBody characterBody)
        {
            var characterBodyTeam = characterBody.Team;
            var id = characterBody.GetId();

            if (TeamToBodies.TryGetValue(characterBodyTeam, out var valueTuple) &&
                GidToBody.TryGetValue(id, out var gidCharacterBody) && characterBody == gidCharacterBody)
            {
                return GidToBody.Remove(id) &&
                       valueTuple.playerBodies.RemoveAIdPointBox(characterBody.IdPointBox);
            }

            return false;
        }

        public TwoDPoint GetEntrancePoint()
        {
            var twoDPoints = Entrance.First().Value.GenPt();
            return twoDPoints;
        }

        public void AddBodiesToStart(IEnumerable<CharacterBody> characterBodies)
        {
            var groupBy = characterBodies.GroupBy(x => x.Team);

            foreach (var grouping in groupBy)
            {
                var team = grouping.Key;
                if (Entrance.TryGetValue(team, out var startPts))
                {
                    var count = grouping.Count();
                    var twoDPoints = startPts.GenPt(count);
                    var valueTuples = grouping.Zip(twoDPoints, (body, point) => (body, point));
                    foreach (var tuple in valueTuples)
                    {
                        var (body, point) = tuple;
                        body.Teleport(point);
                        var key = body.GetId();
                        GidToBody[key] = body;
                        TempTeleMsgToThisMap.Add((key, new TelePortMsg(MgId, point)));
                    }

                    var enumerableToHashSet = grouping.Select(x => x.IdPointBox).IeToHashSet();
                    AddTeamBodies(team, enumerableToHashSet);
                }
                else
                {
                    throw new KeyNotFoundException($"not Such Team Entrance {team}");
                }
            }
        }

        private void AddTeamBodies(int team, HashSet<IdPointBox> enumerableToHashSet)
        {
            if (TeamToBodies.TryGetValue(team, out var qTuple))
            {
                qTuple.playerBodies.AddRangeAabbBoxes(
                    enumerableToHashSet.OfType<IAaBbBox>().IeToHashSet(),
                    CommonConfig.OtherConfig.qspace_max_per_level);
            }
            else
            {
                var playerBodies = SomeTools.CreateEmptyRootBranch(MoveZone);
                var traps = SomeTools.CreateEmptyRootBranch(MoveZone);
                playerBodies.AddRangeAabbBoxes(enumerableToHashSet.OfType<IAaBbBox>().IeToHashSet(),
                    CommonConfig.OtherConfig.qspace_max_per_level);
                var emptyRootBranch = (playerBodies,
                    traps);
                TeamToBodies[team] = emptyRootBranch;
            }
#if DEBUG
            Console.Out.WriteLine($"team {team} add body num :{enumerableToHashSet.Count}");
            Console.Out.WriteLine($"QSpace {team} now have {TeamToBodies[team].playerBodies.Count()}");
#endif
        }


        public void AddBodies(IEnumerable<(CharacterBody characterBody, TwoDPoint pos)> characterBodies)
        {
            var groupBy =
                characterBodies.GroupBy(c => c.characterBody.Team);
            foreach (var valueTuples in groupBy)
            {
                var team = valueTuples.Key;
                foreach (var (characterBody, pos) in valueTuples)
                {
                    characterBody.Teleport(pos);
                    characterBody.MakeProtect((int) CommonConfig.OtherConfig.teleport_protect_time);
                    GidToBody[characterBody.GetId()] = characterBody;
                }

                var enumerableToHashSet = valueTuples.Select(x => x.characterBody.IdPointBox).IeToHashSet();
                AddTeamBodies(team, enumerableToHashSet);
            }
        }

        public void AddBody(CharacterBody characterBody, TwoDPoint? telePos = null)
        {
            var characterBodyTeam = characterBody.Team;
            var characterBodyInBox = characterBody.IdPointBox;
            if (telePos != null) characterBody.Teleport(telePos);
            if (TeamToBodies.TryGetValue(characterBodyTeam, out var tuple))
            {
                tuple.playerBodies.AddAIdPointBox(characterBodyInBox, CommonConfig.OtherConfig.qspace_max_per_level);
            }
            else
            {
                var playerBodies = SomeTools.CreateEmptyRootBranch(MoveZone);
                var traps = SomeTools.CreateEmptyRootBranch(MoveZone);
                playerBodies.AddAIdPointBox(characterBodyInBox, CommonConfig.OtherConfig.qspace_max_per_level);
                var emptyRootBranch = (playerBodies,
                    traps);
                TeamToBodies[characterBodyTeam] = emptyRootBranch;
            }
        }

        public void ActiveApplyDevice()

        {
            static void Active(ApplyDevice applyDevice, bool op)
            {
                applyDevice.IsActive = true;
            }

            MapInteractableThings.ForeachBoxDoWithOutMove<bool, ApplyDevice>(Active, true);
        }

        public void RemoveAllBodies()
        {
            GidToBody.Clear();
            foreach (var (playerBodies, traps) in TeamToBodies.Values)
            {
                playerBodies.Clear();
                traps.Clear();
            }

            // var array = GidToBody.Values.IeToHashSet();
            // var removeBodies = RemoveBodies(array);
            // if (removeBodies.Any())
            // {
            //     throw new Exception("sb not rm success");
            // }
        }
    }
}