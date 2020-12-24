using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class PlayGround
    {
        private string MgId { get; }
        private int ResMId { get; }
        private IPlayRuler PlayRuler { get; }

        private Zone MoveZone { get; }
        private Dictionary<int, (IQSpace playerBodies, IQSpace Traps)> TeamToBodies { get; set; } //角色实体放置到四叉树中，方便子弹碰撞逻辑
        private SightMap SightMap { get; } //视野地图
        private WalkMap WalkMap { get; } //碰撞地图
        public Dictionary<int, CharacterBody> GidToBody { get; } //gid到玩家地图实体对应
        private Dictionary<int, List<IHitMedia>> TeamToHitMedia { get; } // 碰撞媒体缓存，以后可能会有持续多帧的子弹

        private IQSpace MapInteractableThings { get; } // 互动物品，包括地上的武器，道具，被动技能，空载具，售卖机等


        private PlayGround(Dictionary<int, (IQSpace playerBodies, IQSpace Traps)> teamToBodies, SightMap sightMap,
            WalkMap walkMap,
            Dictionary<int, CharacterBody> gidToBody, IQSpace mapInteractableThings, string mgId, IPlayRuler playRuler,
            int resMId, Zone moveZone)
        {
            TeamToBodies = teamToBodies;
            SightMap = sightMap;
            WalkMap = walkMap;
            GidToBody = gidToBody;
            MapInteractableThings = mapInteractableThings;
            MgId = mgId;
            PlayRuler = playRuler;
            ResMId = resMId;
            MoveZone = moveZone;
            TeamToHitMedia = new Dictionary<int, List<IHitMedia>>();
        }


        //初始化状态信息,包括玩家信息和地图信息
        public static (PlayGround playGround, Dictionary<int, HashSet<CharInitMsg>> initMsg) InitPlayGround(
            IEnumerable<CharacterInitData> playerInitData, MapInitData mapInitData, string genMapId, int mapResId,
            IPlayRuler playRuler)
        {
            var bodies = new Dictionary<int, HashSet<CharacterBody>>();
            var characterBodies = new Dictionary<int, CharacterBody>();

            foreach (var initData in playerInitData)
            {
                var initDataTeamId = initData.TeamId;
                if (!mapInitData.TeamToStartPt.TryGetValue(initDataTeamId, out var startPts)) continue;
                var twoDPoint = startPts.GenPt();
                var genCharacterBody = initData.GenCharacterBody(twoDPoint, playRuler);
                if (bodies.TryGetValue(initDataTeamId, out var qSpace))
                {
                    qSpace.Add(genCharacterBody);
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
                    emptyRootBranch.AddIdPointBoxes(aabbBoxShapes, TempConfig.QSpaceBodyMaxPerLevel);
                    return (emptyRootBranch, emptyRootBranch2);
                });
#if DEBUG
            foreach (var qSpace in spaces)
            {
                Console.Out.WriteLine($"init:{qSpace.Key} team : {qSpace.Value.Item1.Count()} body");
            }
#endif

            var playGround = new PlayGround(spaces, mapInitData.SightMap, mapInitData.WalkMap, characterBodies,
                emptyRootBranch, genMapId, playRuler, mapResId, zone);
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


        public (Dictionary<int, IEnumerable<BulletMsg>> gidToBulletsMsg, Dictionary<int, IEnumerable<ISeeTickMsg>>
            playerSeeMsg, IRuleTickResult groundRuleTickResult)
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
            var groundResult = PlayRuler.RulerGoTick(this);

            BodiesQSpaceReplace();
            var playerSee = GetPlayerSee();
            var gidToBulletsMsg = gidToWhichBulletHit.ToDictionary(pair => pair.Key, pair => pair.Value.OfType<Bullet>()
                .Select(x => x.GenMsg()));
            var playerSeeMsg =
                playerSee.ToDictionary(pair => pair.Key, pair => pair.Value.Select(x => x.GenTickMsg(pair.Key)));

            var valueTuple = (gidToBulletsMsg, playerSeeMsg, groundResult);

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


                playerBodies.MoveIdPointBoxes(mapToDicGidToSth, TempConfig.QSpaceBodyMaxPerLevel);
            }
        }


        private Dictionary<int, HashSet<IHitMedia>> HitMediasDo()
        {
            var whoHitGid = new Dictionary<int, HashSet<IHitMedia>>();
            foreach (var ii in TeamToHitMedia)
            {
                var team = ii.Key;
                var bullets = ii.Value;
                foreach (var bullet in bullets)
                {
                    switch (bullet.TargetType)
                    {
                        case ObjType.OtherTeam:

                            foreach (var i in TeamToBodies.Select(kvv => new {kvv, bodyTeam = kvv.Key})
                                .Select(t => new {t, value = t.kvv.Value})
                                .Where(tt => team != tt.t.bodyTeam)
                                .Select(ttt => (bullet.HitTeam(ttt.value.playerBodies),
                                    bullet.HitTeam(ttt.value.Traps)))
                                .SelectMany(x => x.Item1)
                            )
                            {
                                if (whoHitGid.TryGetValue(i, out var hBullets))
                                {
                                    hBullets.Add(bullet);
                                }
                                else
                                {
                                    whoHitGid[i] = new HashSet<IHitMedia> {bullet};
                                }
                            }


                            break;
                        case ObjType.SameTeam:
                            if (TeamToBodies.TryGetValue(team, out var qSpace))
                            {
                                bullet.HitTeam(qSpace.playerBodies);
                                bullet.HitTeam(qSpace.Traps);
                            }

                            break;
                        case ObjType.AllTeam:
                            foreach (var i in from IQSpace value in TeamToBodies
                                select bullet.HitTeam(value)
                                into hitTeam
                                from i in hitTeam
                                select i)
                            {
                                if (whoHitGid.TryGetValue(i, out var hBullets))
                                {
                                    hBullets.Add(bullet);
                                }
                                else
                                {
                                    whoHitGid[i] = new HashSet<IHitMedia> {bullet};
                                }
                            }

                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                bullets.RemoveAll(x => !x.CanGoNextTick());
            }

            return whoHitGid;
        }

        private static readonly Func<IAaBbBox, bool> IsS = x => x is ApplyBox;


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
                        MapInteractableThings.AddSingleAaBbBox(aInteractable, TempConfig.QSpaceBodyMaxPerLevel);
                    }

                    var whoPickCageCall = aCharGoTickMsg.WhoInteractCall;
                    if (whoPickCageCall != null)
                    {
                        (IAaBbBox? singleBox, bool needContinueCall) = aCharGoTickMsg.MapInteractive switch
                        {
                            MapInteract.RecycleCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                whoPickCageCall.GetAnchor(), IsC), true),
                            MapInteract.InVehicleCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                whoPickCageCall.GetAnchor(), IsV), false),
                            MapInteract.PickCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                whoPickCageCall.GetAnchor(), IsC), false),
                            MapInteract.KickVehicleCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                whoPickCageCall.GetAnchor(), IsV), true),
                            MapInteract.GetInfoCall => (
                                MapInteractableThings.InteractiveFirstSingleBox(whoPickCageCall.GetAnchor(), IsS),
                                false),
                            MapInteract.BuyOrApplyCall => (MapInteractableThings.InteractiveFirstSingleBox(
                                whoPickCageCall.GetAnchor(), IsS), true),
                            null => throw new ArgumentOutOfRangeException(nameof(aCharGoTickMsg)),
                            _ => throw new ArgumentOutOfRangeException(nameof(aCharGoTickMsg))
                        };

                        switch (singleBox)
                        {
                            case null:
                                break;
                            case IMapInteractable mapInteractable:
                                if (needContinueCall) mapInteractable.StartActTwoBySomeBody(whoPickCageCall);
                                else mapInteractable.StartActOneBySomeBody(whoPickCageCall);
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

                traps.AddIdPointBoxes(idPointBoxesToAdd, TempConfig.QSpaceBodyMaxPerLevel, true);
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

        public void AddBody(CharacterBody characterBody, TwoDPoint telePos)
        {
            var characterBodyTeam = characterBody.Team;
            var characterBodyInBox = characterBody.InBox;
            if (TeamToBodies.TryGetValue(characterBodyTeam, out var tuple))
            {
                characterBody.ReLocate(telePos);

                tuple.playerBodies.AddAIdPointBox(characterBodyInBox, TempConfig.QSpaceBodyMaxPerLevel);
            }
            else
            {
                var playerBodies = SomeTools.CreateEmptyRootBranch(MoveZone);
                var traps = SomeTools.CreateEmptyRootBranch(MoveZone);
                playerBodies.AddAIdPointBox(characterBodyInBox, TempConfig.QSpaceBodyMaxPerLevel);
                var emptyRootBranch = (playerBodies,
                    traps);
                TeamToBodies[characterBodyTeam] = emptyRootBranch;
            }
        }

        public (PveResultType pveResultType, IEnumerable<CharacterBody>? deadBodies) CheckPve(PveWinCond pveWinCond)
        {
            var groupBy = GidToBody.Values.GroupBy(x => x.BodyMark);
            var creepClear = false;
            var bossClear = false;
            IEnumerable<CharacterBody>? deadBodies = null;
            foreach (var characterBodies in groupBy)
            {
                switch (characterBodies.Key)
                {
                    case BodyMark.Player:
                        deadBodies = characterBodies.Where(x => x.CharacterStatus.SurvivalStatus.IsDead());
                        var all = characterBodies.All(x => x.CharacterStatus.SurvivalStatus.IsDead());
                        if (all)
                        {
                            return (PveResultType.PveLoss, deadBodies);
                        }

                        break;
                    case BodyMark.Creep:
                        var all2 = characterBodies.All(x => x.CharacterStatus.SurvivalStatus.IsDead());
                        if (all2)
                        {
                            creepClear = true;
                        }

                        break;
                    case BodyMark.Boss:
                        var all3 = characterBodies.All(x => x.CharacterStatus.SurvivalStatus.IsDead());
                        if (all3)
                        {
                            bossClear = true;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return pveWinCond switch
            {
                PveWinCond.AllClear => bossClear && creepClear
                    ? (PveResultType.PveWin, deadBodies)
                    : (PveResultType.NotFinish, deadBodies),
                PveWinCond.BossClear => bossClear
                    ? (PveResultType.PveWin, deadBodies)
                    : (PveResultType.NotFinish, deadBodies),
                _ => throw new ArgumentOutOfRangeException(nameof(pveWinCond), pveWinCond, null)
            };
        }
    }
}