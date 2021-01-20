using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using collision_and_rigid;

namespace game_stuff
{
    public readonly struct PlayGroundGoTickResult
    {
        public PlayGroundGoTickResult(ImmutableDictionary<int, ImmutableHashSet<HitResult>> playerBeHit,
            ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<HitResult>>> trapBeHit,
            ImmutableDictionary<int, ImmutableHashSet<ISeeTickMsg>> playerSee,
            ImmutableDictionary<int, int> playerTeleportTo)
        {
            PlayerBeHit = playerBeHit;
            TrapBeHit = trapBeHit;
            PlayerSee = playerSee;
            PlayerTeleportTo = playerTeleportTo;
        }

        public ImmutableDictionary<int, int> PlayerTeleportTo { get; }
        public ImmutableDictionary<int, ImmutableHashSet<HitResult>> PlayerBeHit { get; }
        public ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<HitResult>>> TrapBeHit { get; }
        public ImmutableDictionary<int, ImmutableHashSet<ISeeTickMsg>> PlayerSee { get; }


        public static PlayGroundGoTickResult Sum(IEnumerable<PlayGroundGoTickResult> playGroundGoTickResults)
        {
            var hit = new Dictionary<int, ImmutableHashSet<HitResult>>();
            var trap = new Dictionary<int, ImmutableDictionary<int, ImmutableHashSet<HitResult>>>();
            var see = new Dictionary<int, ImmutableHashSet<ISeeTickMsg>>();
            var ints = new Dictionary<int, int>();
            var (hit1, trap1, see2, dic) =
                playGroundGoTickResults.Aggregate((hit, trap, see, ints), (s, x) =>
                {
                    var (dictionary, dictionary1, see1, ints) = s;
                    var keyValuePairs = dictionary1.Union(x.TrapBeHit);
                    var valuePairs = dictionary.Union(x.PlayerBeHit);
                    var enumerable = see1.Union(x.PlayerSee);
                    var union = ints.Union(x.PlayerTeleportTo);
                    return ((Dictionary<int, ImmutableHashSet<HitResult>> hit,
                        Dictionary<int, ImmutableDictionary<int, ImmutableHashSet<HitResult>>> trap,
                        Dictionary<int, ImmutableHashSet<ISeeTickMsg>> see, Dictionary<int, int> ints)) (valuePairs,
                        keyValuePairs, enumerable, union);
                });
            return new PlayGroundGoTickResult(hit1.ToImmutableDictionary(), trap1.ToImmutableDictionary(),
                see2.ToImmutableDictionary(), dic.ToImmutableDictionary());
        }

        public void Deconstruct(out ImmutableDictionary<int, ImmutableHashSet<HitResult>> playerBeHit,
            out ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<HitResult>>> trapBeHit,
            out ImmutableDictionary<int, ImmutableHashSet<ISeeTickMsg>> playerSee,
            out ImmutableDictionary<int, int> playerTeleportTo)
        {
            playerSee = PlayerSee;
            trapBeHit = TrapBeHit;
            playerBeHit = PlayerBeHit;
            playerTeleportTo = PlayerTeleportTo;
        }
    }


    public class PlayGround
    {
        public int MgId { get; }
        public int ResMId { get; }
        private Zone MoveZone { get; }
        private Dictionary<int, (IQSpace playerBodies, IQSpace Traps)> TeamToBodies { get; } //角色实体放置到四叉树中，方便子弹碰撞逻辑
        private SightMap SightMap { get; } //视野地图
        private WalkMap WalkMap { get; } //碰撞地图
        private Dictionary<int, CharacterBody> GidToBody { get; } //gid到玩家地图实体对应
        private Dictionary<int, List<IHitMedia>> TeamToHitMedia { get; } // 碰撞媒体缓存，以后可能会有持续多帧的子弹

        private IQSpace MapInteractableThings { get; } // 互动物品，包括地上的武器，道具，被动技能，空载具，售卖机等


        private PlayGround(Dictionary<int, (IQSpace playerBodies, IQSpace Traps)> teamToBodies, SightMap sightMap,
            WalkMap walkMap,
            Dictionary<int, CharacterBody> gidToBody, IQSpace mapInteractableThings, int mgId,
            int resMId, Zone moveZone)
        {
            TeamToBodies = teamToBodies;
            SightMap = sightMap;
            WalkMap = walkMap;
            GidToBody = gidToBody;
            MapInteractableThings = mapInteractableThings;
            MgId = mgId;
            ResMId = resMId;
            MoveZone = moveZone;
            TeamToHitMedia = new Dictionary<int, List<IHitMedia>>();
        }


        //初始化状态信息,包括玩家信息和地图信息
        public static (PlayGround playGround, Dictionary<int, HashSet<CharInitMsg>> initMsg) InitPlayGround(
            IEnumerable<CharacterInitData> playerInitData, MapInitData mapInitData, int genMapId, int mapResId)
        {
            var bodies = new Dictionary<int, HashSet<CharacterBody>>();
            var characterBodies = new Dictionary<int, CharacterBody>();

            foreach (var initData in playerInitData)
            {
                var initDataTeamId = initData.TeamId;
                if (!mapInitData.TeamToStartPt.TryGetValue(initDataTeamId, out var startPts))
                {
                    throw new DirectoryNotFoundException($"not have such team start{initDataTeamId}");
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
                        SomeTools.EnumerableToHashSet(hashSet.Select(x => x.InBox));
                    emptyRootBranch.AddIdPointBoxes(aabbBoxShapes, LocalConfig.QSpaceBodyMaxPerLevel);
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
                emptyRootBranch3, genMapId, mapResId, zone);

            foreach (var applyDevice in mapInitData.StandardMapInteractableList)
            {
                applyDevice.SetInPlayGround(playGround);
            }

            var aaBbBoxes = SomeTools.EnumerableToHashSet(mapInitData.StandardMapInteractableList.OfType<IAaBbBox>());
            playGround.MapInteractableThings.AddRangeAabbBoxes(aaBbBoxes,
                LocalConfig.QSpaceBodyMaxPerLevel);

            return (playGround, playGround.GenInitMsg());
        }

        // 初始化状态消息输出
        private Dictionary<int, HashSet<CharInitMsg>> GenInitMsg()
        {
            var dictionary = new Dictionary<int, HashSet<CharInitMsg>>();
            var charInitMsgs =
                GidToBody.ToDictionary(p => p.Key,
                    p => p.Value.GenInitMsg());
            foreach (var kv in charInitMsgs)
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
            var teleports = new Dictionary<int, int>();
            var everyBodyGoATick = EveryTeamGoATick(gidToOperates, teleports);

            MapInteractableGoATick();

            var (playerBeHit, trapBeHit, teamRadarSee) = HitMediasDo();

            foreach (var twoDTwoP in everyBodyGoATick)
            {
                if (GidToBody.TryGetValue(twoDTwoP.Key, out var characterBody))
                {
                    characterBody.Move(twoDTwoP.Value);
                }
            }

            BodiesQSpaceReplace();
            var playerSee = GetPlayerSee();
            var playerSeeMsg =
                playerSee.ToImmutableDictionary(pair => pair.Key,
                    pair =>
                    {
                        var gid = pair.Key;
                        var seeTickMsgs = pair.Value.Select(x => x.GenTickMsg(gid));
                        if (GidToBody.TryGetValue(gid, out var cb) &&
                            teamRadarSee.TryGetValue(cb.Team, out var sees))
                            return seeTickMsgs.Union(sees).ToImmutableHashSet();
                        return seeTickMsgs
                            .ToImmutableHashSet();
                    });

            var valueTuple =
                new PlayGroundGoTickResult(playerBeHit, trapBeHit, playerSeeMsg, teleports.ToImmutableDictionary());

            return valueTuple;
        }

        private void MapInteractableGoATick()
        {
            var mapInteractables = new List<IHitMedia>();

            var vehicleCanIns = new List<VehicleCanIn>();

            var tuples = new List<(TwoDPoint pos, Weapon[] weapons)>();

            static void Act(VehicleCanIn vehicleCanIn,
                (List<IHitMedia> bullets, List<VehicleCanIn>vehicleCanIns, List<(TwoDPoint pos, Weapon[] weapons)>
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

            MapInteractableThings
                .ForeachBoxDoWithOutMove<(List<IHitMedia> bullets,
                    List<VehicleCanIn> vehicleCanIns,
                    List<(TwoDPoint pos, Weapon[] weapons)> weaponsDrop
                    ), VehicleCanIn>(Act,
                    (mapInteractables, vehicleCanIns, tuples));
            TeamToHitMedia[-1] = mapInteractables;

            foreach (var vehicleCanIn in vehicleCanIns)
            {
                MapInteractableThings.RemoveSingleAaBbBox(vehicleCanIn);
            }

            var selectMany = tuples.SelectMany(x => x.weapons.Select(w => (IAaBbBox) w.DropAsIMapInteractable(x.pos)));
            var enumerableToHashSet = SomeTools.EnumerableToHashSet(selectMany);
            MapInteractableThings.AddRangeAabbBoxes(enumerableToHashSet, LocalConfig.QSpaceBodyMaxPerLevel);
        }


        private Dictionary<int, HashSet<ICanBeSaw>> GetPlayerSee()
        {
            var gidToCharacterBodies = new Dictionary<int, HashSet<ICanBeSaw>>();
            foreach (var kv in GidToBody)
            {
                var key = kv.Key;
                var characterBody = kv.Value;
                var sightZone = characterBody.GetSightZone();
// #if DEBUG
//                 Console.Out.WriteLine($"gid::{characterBody.GetId()}");
// #endif
                var characterBodies = new HashSet<ICanBeSaw>();

                var filterToBoxList = MapInteractableThings.FilterToBoxList<ICanBeSaw, CharacterBody>(
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
                            qSpace.FilterToGIdPsList((x, y) => true, true).OfType<ICanBeSaw>();

                        var ofType = traps.FilterToGIdPsList((a, b) => true, true)
                            .OfType<ICanBeSaw>();
                        // #if DEBUG
//                         Console.Out.WriteLine($"list::{filterToGIdPsList.ToArray().Length}");
// #endif                
                        characterBodies.UnionWith(filterToGIdPsList);
                        characterBodies.UnionWith(ofType);
                    }
                    else
                    {
                        var filterToGIdPsList =
                            qSpace.FilterToGIdPsList((idp, acb) => acb.InSight(idp, SightMap),
                                characterBody, sightZone);
                        var bodies = filterToGIdPsList.OfType<CharacterBody>();

                        var trapsSee =
                            traps.FilterToGIdPsList((idp, acb) => acb.InSight(idp, SightMap),
                                    characterBody, sightZone)
                                .OfType<Trap>()
                                .Where(t => t.CanBeSee);
// #if DEBUG
//                         Console.Out.WriteLine($" {characterBody.Team}:other:{bTeam}::{qSpace.Count()}::{bodies.Count()}");
// #endif

                        characterBodies.UnionWith(bodies);
                        characterBodies.UnionWith(trapsSee);
                    }
                }

                gidToCharacterBodies[key] = characterBodies;
            }

            return gidToCharacterBodies;
        }

        private void BodiesQSpaceReplace()
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


                playerBodies.MoveIdPointBoxes(mapToDicGidToSth, LocalConfig.QSpaceBodyMaxPerLevel);
            }
        }

        private static void RecordHitResult(HitResult hitResult,
            IDictionary<int, HashSet<HitResult>> gidD, IDictionary<int, Dictionary<int, HashSet<HitResult>>> tidD,
            IDictionary<int, HashSet<ISeeTickMsg>> teamRadarSee)
        {
            if (hitResult.HitMedia is RadarWave)
            {
                var id = hitResult.CasterOrOwner.Team;
                var twoDPoint = hitResult.HitBody.GetAnchor();
                var bodySize = hitResult.HitBody.GetSize();

                var radarSeeMsg = new RadarSeeMsg(twoDPoint, bodySize);
                if (teamRadarSee.TryGetValue(id, out var seeTickMsgs))
                {
                    seeTickMsgs.Add(radarSeeMsg);
                }
                else
                {
                    teamRadarSee[id] = new HashSet<ISeeTickMsg> {radarSeeMsg};
                }

                return;
            }

            switch (hitResult.HitBody)
            {
                case CharacterBody characterBody:
                    var key = characterBody.GetId();
                    if (gidD.TryGetValue(key, out var hBullets))
                    {
                        hBullets.Add(hitResult);
                    }
                    else
                    {
                        gidD[key] = new HashSet<HitResult> {hitResult};
                    }

                    break;
                case Trap trap:
                    var gid = trap.GetFinalCaster().GetId();
                    var tid = trap.GetId();
                    if (tidD.TryGetValue(gid, out var hBullets2))
                    {
                        if (hBullets2.TryGetValue(tid, out hBullets))
                        {
                            hBullets.Add(hitResult);
                        }
                        else
                        {
                            hBullets2[tid] = new HashSet<HitResult> {hitResult};
                        }
                    }
                    else
                    {
                        var dictionary = new Dictionary<int, HashSet<HitResult>>
                        {
                            [tid] = new HashSet<HitResult> {hitResult}
                        };
                        tidD[tid] = dictionary;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private (ImmutableDictionary<int, ImmutableHashSet<HitResult>> gidBeHit,
            ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<HitResult>>> gidTrapBeHit,
            Dictionary<int, HashSet<ISeeTickMsg>> teamRadarSee)
            HitMediasDo()
        {
            var gidHitBy = new Dictionary<int, HashSet<HitResult>>();
            var tidHitBy = new Dictionary<int, Dictionary<int, HashSet<HitResult>>>();
            var teamRadarSee = new Dictionary<int, HashSet<ISeeTickMsg>>();
            foreach (var ii in TeamToHitMedia)
            {
                var team = ii.Key;
                var hitMedias = ii.Value;
                foreach (var bullet in hitMedias)
                {
                    switch (bullet.TargetType)
                    {
                        case ObjType.OtherTeam:

                            var selectMany = TeamToBodies.Where(pair => pair.Key != team)
                                .SelectMany(x => hitMedias.SelectMany(aHitM => aHitM.HitTeam(x.Value.playerBodies)
                                    .Union(aHitM.HitTeam(x.Value.Traps))));

                            foreach (var hr in selectMany)
                            {
                                RecordHitResult(hr, gidHitBy, tidHitBy, teamRadarSee);
                            }

                            break;
                        case ObjType.SameTeam:
                            if (TeamToBodies.TryGetValue(team, out var qSpace))
                            {
                                var enumerable = bullet.HitTeam(qSpace.playerBodies)
                                    .Union(bullet.HitTeam(qSpace.Traps));
                                foreach (var hitResult in enumerable)
                                {
                                    RecordHitResult(hitResult, gidHitBy, tidHitBy, teamRadarSee);
                                }
                            }

                            break;
                        case ObjType.AllTeam:
                            var results = TeamToBodies.Values.SelectMany(x =>
                                hitMedias.SelectMany(bb => bb.HitTeam(x.playerBodies).Union(bb.HitTeam(x.Traps))));

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

            var gidBeHit = gidHitBy.ToImmutableDictionary(pair => pair.Key, pair => pair.Value.ToImmutableHashSet());
            var gidTrapBeHit = tidHitBy.ToImmutableDictionary(pa => pa.Key,
                pa => pa.Value.ToImmutableDictionary(pp => pp.Key,
                    pp => pp.Value.ToImmutableHashSet()));
            return (gidBeHit, gidTrapBeHit, teamRadarSee);
        }

        private static readonly Func<IAaBbBox, bool> IsS = x => x is ApplyDevice;


        private static readonly Func<IAaBbBox, bool> IsC = x => x is CageCanPick;


        private static readonly Func<IAaBbBox, bool> IsV = x => x is VehicleCanIn;

        private Dictionary<int, ITwoDTwoP> EveryTeamGoATick(Dictionary<int, Operate> gidToOperates,
            Dictionary<int, int> dictionary)
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

                    if (!stillAlive)
                    {
                        if (goTickResult.Self?.IdPointBox != null) thisTeamToRemove.Add(goTickResult.Self.IdPointBox);
                        continue;
                    }

                    var launchBullet = goTickResult.LaunchBullet;
                    if (launchBullet == null) continue;

                    var addBulletToDict = AddBulletToDict(team, launchBullet);
                    if (addBulletToDict != null) idPointBoxesToAdd.Add(addBulletToDict);
                }

                traps.RemoveIdPointBoxes(thisTeamToRemove);

                foreach (var gtb in mapToDicGidToSth)
                {
                    // (gid, (twoDTwoP, bullet))

                    var gid = gtb.Key;
                    var aCharGoTickMsg = gtb.Value;
                    var twoDTwoP = aCharGoTickMsg.Move;
                    var stillAlive = aCharGoTickMsg.StillActive;
                    var teleportToMapId = aCharGoTickMsg.TeleportToMapId;

                    if (!stillAlive)
                    {
                        continue;
                    }

                    if (teleportToMapId != null)
                    {
                        dictionary[gid] = teleportToMapId.Value;
                    }

                    if (twoDTwoP != null)
                    {
#if DEBUG
                        Console.Out.WriteLine(
                            $" {twoDTwoP.GetType().TypeHandle.Value.ToString()} :: move res :: {twoDTwoP.ToString()}");
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

                    var mapInteractive = aCharGoTickMsg.DropThing;
                    foreach (var aInteractable in mapInteractive)
                    {
                        MapInteractableThings.AddSingleAaBbBox(aInteractable, LocalConfig.QSpaceBodyMaxPerLevel);
                    }

                    var interactCaller = aCharGoTickMsg.WhoInteractCall;
                    if (interactCaller != null)
                    {
                        (IAaBbBox? singleBox, bool needContinueCall) = aCharGoTickMsg.MapInteractive switch
                        {
                            MapInteract.RecycleCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                interactCaller.GetAnchor(), IsC), true),
                            MapInteract.InVehicleCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                interactCaller.GetAnchor(), IsV), false),
                            MapInteract.PickCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                interactCaller.GetAnchor(), IsC), false),
                            MapInteract.KickVehicleCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                interactCaller.GetAnchor(), IsV), true),
                            MapInteract.GetInfoCall => (
                                MapInteractableThings.InteractiveFirstSingleBox(interactCaller.GetAnchor(), IsS),
                                false),
                            MapInteract.BuyOrApplyCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                interactCaller.GetAnchor(), IsS), true),
                            null => throw new ArgumentOutOfRangeException(nameof(aCharGoTickMsg)),
                            _ => throw new ArgumentOutOfRangeException(nameof(aCharGoTickMsg))
                        };

                        switch (singleBox)
                        {
                            case null:
                                break;
                            case IMapInteractable mapInteractable:
                                if (needContinueCall) mapInteractable.StartActTwoBySomeBody(interactCaller);
                                else mapInteractable.StartActOneBySomeBody(interactCaller);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(singleBox));
                        }
                    }

                    var bullet = aCharGoTickMsg.LaunchBullet;
                    if (bullet == null) continue;
                    var addBulletToDict = AddBulletToDict(team, bullet);
                    if (addBulletToDict != null) idPointBoxesToAdd.Add(addBulletToDict);
                }

                traps.AddIdPointBoxes(idPointBoxesToAdd, LocalConfig.QSpaceBodyMaxPerLevel, true);
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

                    var aTrap = summons.SetATrap();
                    return aTrap;

                default:
                    throw new ArgumentOutOfRangeException(nameof(bullet));
            }
        }

        public bool RemoveBody(CharacterBody characterBody)
        {
            var characterBodyTeam = characterBody.Team;
            var id = characterBody.GetId();

            if (TeamToBodies.TryGetValue(characterBodyTeam, out var valueTuple) &&
                GidToBody.TryGetValue(id, out var gidCharacterBody) && characterBody == gidCharacterBody)
            {
                return GidToBody.Remove(id) &&
                       valueTuple.playerBodies.RemoveAIdPointBox(characterBody.InBox);
            }

            return false;
        }

        public void AddBodies(IEnumerable<(CharacterBody characterBody, TwoDPoint pos)> characterBodies)
        {
            var groupBy = characterBodies.GroupBy(c => c.characterBody.Team);

            foreach (var valueTuples in groupBy)
            {
                var team = valueTuples.Key;
                foreach (var (characterBody, pos) in valueTuples)
                {
                    characterBody.Teleport(pos);
                }

                var enumerableToHashSet = SomeTools.EnumerableToHashSet(valueTuples.Select(x => x.characterBody.InBox));
                if (TeamToBodies.TryGetValue(team, out var qTuple))
                {
                    qTuple.playerBodies.AddIdPointBoxes(
                        enumerableToHashSet,
                        LocalConfig.QSpaceBodyMaxPerLevel);
                }
                else
                {
                    var playerBodies = SomeTools.CreateEmptyRootBranch(MoveZone);
                    var traps = SomeTools.CreateEmptyRootBranch(MoveZone);
                    playerBodies.AddIdPointBoxes(enumerableToHashSet, LocalConfig.QSpaceBodyMaxPerLevel);
                    var emptyRootBranch = (playerBodies,
                        traps);
                    TeamToBodies[team] = emptyRootBranch;
                }
            }
        }

        public void AddBody(CharacterBody characterBody, TwoDPoint telePos)
        {
            var characterBodyTeam = characterBody.Team;
            var characterBodyInBox = characterBody.InBox;
            characterBody.Teleport(telePos);
            if (TeamToBodies.TryGetValue(characterBodyTeam, out var tuple))
            {
                tuple.playerBodies.AddAIdPointBox(characterBodyInBox, LocalConfig.QSpaceBodyMaxPerLevel);
            }
            else
            {
                var playerBodies = SomeTools.CreateEmptyRootBranch(MoveZone);
                var traps = SomeTools.CreateEmptyRootBranch(MoveZone);
                playerBodies.AddAIdPointBox(characterBodyInBox, LocalConfig.QSpaceBodyMaxPerLevel);
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
    }
}