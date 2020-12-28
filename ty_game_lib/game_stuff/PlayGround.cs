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
    public class PlayGround
    {
        private int MgId { get; }
        private int ResMId { get; }
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
            IEnumerable<CharacterInitData> playerInitData, MapInitData mapInitData, int genMapId, int mapResId,
            LevelUps levelUps)
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
                var genCharacterBody = initData.GenCharacterBody(twoDPoint, levelUps);
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

            var aaBbBoxes = SomeTools.EnumerableToHashSet(mapInitData.StandardMapInteractableList.Cast<IAaBbBox>());
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


        public ((Dictionary<int, HashSet<HitResult>> playerBehit, Dictionary<int, HashSet<HitResult>> trapBehit)
            gidToWhichBulletHit, Dictionary<int, IEnumerable<ISeeTickMsg>> playerSeeMsg)
            PlayGroundGoATick(
                Dictionary<int, Operate> gidToOperates)
        {
            var everyBodyGoATick = EveryTeamGoATick(gidToOperates);
            MapInteractableGoATick();

            var gidToWhichBulletHit = HitMediasDo();

            foreach (var twoDTwoP in everyBodyGoATick)
            {
                if (GidToBody.TryGetValue(twoDTwoP.Key, out var characterBody))
                {
                    characterBody.Move(twoDTwoP.Value);
                }
            }

            // foreach (var qSpace in TeamToBodies.Select(kk => kk.Value))
            // {
            //     qSpace.MoveIdPoint(everyBodyGoATick, TempConfig.QSpaceBodyMaxPerLevel);
            // }

            BodiesQSpaceReplace();
            var playerSee = GetPlayerSee();
            var playerSeeMsg =
                playerSee.ToDictionary(pair => pair.Key, pair => pair.Value.Select(x => x.GenTickMsg(pair.Key)));

            var valueTuple = (gidToWhichBulletHit, playerSeeMsg);

            return valueTuple;
        }

        private void MapInteractableGoATick()
        {
            var mapInteractables = new List<IHitMedia>();

            var vehicleCanIns = new List<VehicleCanIn>();


            static void Act(VehicleCanIn vehicleCanIn,
                (List<IHitMedia> bullets, List<VehicleCanIn>vehicleCanIns) valueTuple)
            {
                var goATick = vehicleCanIn.GoATick();

                if (goATick == null) return;
                var (bullets, vehicleCanIns) = valueTuple;
                bullets.Add(goATick);
                vehicleCanIns.Add(vehicleCanIn);
            }

            MapInteractableThings
                .ForeachBoxDoWithOutMove<(List<IHitMedia> bullets, List<VehicleCanIn>vehicleCanIns), VehicleCanIn>(Act,
                    (mapInteractables, vehicleCanIns));
            TeamToHitMedia[-1] = mapInteractables;

            foreach (var vehicleCanIn in vehicleCanIns)
            {
                MapInteractableThings.RemoveSingleAaBbBox(vehicleCanIn);
            }
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

        static void RecordHitResult(HitResult hitResult,
            Dictionary<int, HashSet<HitResult>> gidD, Dictionary<int, HashSet<HitResult>> tidD)
        {
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
                    var key2 = trap.GetId();
                    if (tidD.TryGetValue(key2, out var hBullets2))
                    {
                        hBullets2.Add(hitResult);
                    }
                    else
                    {
                        tidD[key2] = new HashSet<HitResult> {hitResult};
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private (Dictionary<int, HashSet<HitResult>>playerBehit, Dictionary<int, HashSet<HitResult>>trapBehit)
            HitMediasDo()
        {
            var gidHitBy = new Dictionary<int, HashSet<HitResult>>();
            var tidHitBy = new Dictionary<int, HashSet<HitResult>>();
            foreach (var ii in TeamToHitMedia)
            {
                var team = ii.Key;
                var bullets = ii.Value;
                foreach (var bullet in bullets)
                {
                    switch (bullet.TargetType)
                    {
                        case ObjType.OtherTeam:

                            var selectMany = TeamToBodies.Where(pair => pair.Key != team)
                                .SelectMany(x => bullets.SelectMany(aHitM => aHitM.HitTeam(x.Value.playerBodies)
                                    .Union(aHitM.HitTeam(x.Value.Traps))));

                            foreach (var hr in selectMany)
                            {
                                RecordHitResult(hr, gidHitBy, tidHitBy);
                            }

                            break;
                        case ObjType.SameTeam:
                            if (TeamToBodies.TryGetValue(team, out var qSpace))
                            {
                                var enumerable = bullet.HitTeam(qSpace.playerBodies)
                                    .Union(bullet.HitTeam(qSpace.Traps));
                                foreach (var hitResult in enumerable)
                                {
                                    RecordHitResult(hitResult, gidHitBy, tidHitBy);
                                }
                            }

                            break;
                        case ObjType.AllTeam:
                            var results = TeamToBodies.Values.SelectMany(x =>
                                bullets.SelectMany(bb => bb.HitTeam(x.playerBodies).Union(bb.HitTeam(x.Traps))));

                            foreach (var hitResult in results)
                            {
                                RecordHitResult(hitResult, gidHitBy, tidHitBy);
                            }

                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                bullets.RemoveAll(x => !x.CanGoNextTick());
            }

            return (gidHitBy, tidHitBy);
        }

        private static readonly Func<IAaBbBox, bool> IsS = x => x is ApplyDevice;


        private static readonly Func<IAaBbBox, bool> IsC = x => x is CageCanPick;


        private static readonly Func<IAaBbBox, bool> IsV = x => x is VehicleCanIn;

        private Dictionary<int, ITwoDTwoP> EveryTeamGoATick(Dictionary<int, Operate> gidToOperates)
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


                    if (!stillAlive)
                    {
                        continue;
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
                foreach (var valueTuple in valueTuples)
                {
                    valueTuple.characterBody.ReLocate(valueTuple.pos);
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
            characterBody.ReLocate(telePos);
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