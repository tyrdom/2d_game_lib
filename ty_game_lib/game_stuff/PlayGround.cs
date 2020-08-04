using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class PlayGround
    {
        private readonly Dictionary<int, IQSpace> TeamToBodies; //角色放置到四叉树中
        private readonly SightMap _sightMap; //视野地图
        private readonly WalkMap _walkMap; //碰撞地图
        private Dictionary<int, CharacterBody> GidToBody; //gid到玩家地图实体对应
        private Dictionary<int, List<IHitStuff>> TeamToBullet;

        // private TempConfig TempConfig { get; }

        public PlayGround(Dictionary<int, IQSpace> teamToBodies, SightMap sightMap, WalkMap walkMap,
            Dictionary<int, CharacterBody> gidToBody)
        {
            TeamToBodies = teamToBodies;
            _sightMap = sightMap;
            _walkMap = walkMap;
            GidToBody = gidToBody;
            TeamToBullet = new Dictionary<int, List<IHitStuff>>();
        }

        //初始化状态信息,包括玩家信息和地图信息
        public static (PlayGround, Dictionary<int, HashSet<CharInitMsg>>) InitPlayGround(
            IEnumerable<PlayerInitData> playerInitData, MapInitData mapInitData)
        {
            var bodies = new Dictionary<int, HashSet<CharacterBody>>();
            var characterBodies = new Dictionary<int, CharacterBody>();

            foreach (var initData in playerInitData)
            {
                var initDataTeamId = initData.TeamId;
                if (mapInitData.TeamToStartPt.TryGetValue(initDataTeamId, out var startPts))
                {
                    var twoDPoint = startPts.GenPt();
                    var genCharacterBody = initData.GenCharacterBody(twoDPoint);
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
            }

            var spaces = bodies.ToDictionary(p => p.Key,
                p =>
                {
                    var hashSet = p.Value;
                    var zone = mapInitData.GetZone();
                    var emptyRootBranch = SomeTools.CreateEmptyRootBranch(zone);
                    var aabbBoxShapes =
                        SomeTools.ListToHashSet(hashSet.Select(x => x.CovToAabbPackBox()));
                    emptyRootBranch.AddIdPoint(aabbBoxShapes, TempConfig.QSpaceBodyMaxPerLevel);
                    return emptyRootBranch;
                });
#if DEBUG
            foreach (var qSpace in spaces)
            {
                Console.Out.WriteLine($"init:{qSpace.Key} team : {qSpace.Value.Count()} body");
            }
#endif
            var playGround = new PlayGround(spaces, mapInitData.SightMap, mapInitData.WalkMap, characterBodies);
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
                if (dictionary.TryGetValue(kv.Key, out var charInitMsglist))
                {
                    charInitMsglist.Add(kv.Value);
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
                if (dictionary.TryGetValue(characterBody.Team, out var gidToOp
                    )
                )
                {
                    gidToOp[kv.Key] = kv.Value;
                }
                else
                {
                    dictionary[characterBody.Team] = new Dictionary<int, Operate>() {{kv.Key, kv.Value}};
                }
            }

            return dictionary;
        }

        public (Dictionary<int, IEnumerable<BulletMsg>>, Dictionary<int, IEnumerable<CharTickMsg>>
            ) PlayGroundGoATick(Dictionary<int, Operate> gidToOperates)
        {
            var everyBodyGoATick = EveryBodyGoATick(gidToOperates);
            var gidToWhichBulletHit = BulletsDo();
            foreach (var qSpace in TeamToBodies.Select(kk => kk.Value))
            {
                qSpace.MoveIdPoint(everyBodyGoATick, TempConfig.QSpaceBodyMaxPerLevel);
            }

            BodiesRePlace();
            var playerSee = GetPlayerSee();
            var gidToBulletsMsg = gidToWhichBulletHit.ToDictionary(pair => pair.Key, pair => pair.Value
                .Select(x => x.GenMsg()));
            var playerSeeMsg = playerSee.ToDictionary(pair => pair.Key, pair => pair.Value.Select(x => x.GenTickMsg()));
            var valueTuple = (gidToBulletsMsg, playerSeeMsg);
            return valueTuple;
        }

        private Dictionary<int, HashSet<CharacterBody>> GetPlayerSee()
        {
            var gidToCharacterBodies = new Dictionary<int, HashSet<CharacterBody>>();
            foreach (var kv in GidToBody)
            {
                var key = kv.Key;
                var characterBody = kv.Value;
#if DEBUG
                Console.Out.WriteLine($"gid::{characterBody.GetId()}");
#endif
                var characterBodies = new HashSet<CharacterBody>();
                foreach (var kv2 in TeamToBodies)
                {
                    var bTeam = kv2.Key;
                    var qSpace = kv2.Value;
#if DEBUG
                    Console.Out.WriteLine($"team{bTeam}:have:{qSpace.Count()} body");
#endif
                    if (characterBody.Team == bTeam)
                    {
                        var filterToGIdPsList =
                            qSpace.FilterToGIdPsList((x, y) => true, true).OfType<CharacterBody>();
// #if DEBUG
//                         Console.Out.WriteLine($"list::{filterToGIdPsList.ToArray().Length}");
// #endif
                        characterBodies.UnionWith(filterToGIdPsList);
                    }
                    else
                    {

                        var filterToGIdPsList =
                            qSpace.FilterToGIdPsList((idp, acb) => acb.InSight(idp, _sightMap),
                                characterBody);
                        var bodies = filterToGIdPsList.OfType<CharacterBody>();
                        
#if DEBUG
                        Console.Out.WriteLine($" {characterBody.Team}:other:{bTeam}::{qSpace.Count()}::{bodies.Count()}");
#endif
                        characterBodies.UnionWith(bodies);
                    }
                }

                gidToCharacterBodies[key] = characterBodies;
            }

            return gidToCharacterBodies;
        }

        public void BodiesRePlace()
        {
            foreach (var qSpace in TeamToBodies.Select(kv => kv.Value))
            {
                var mapToDicGidToSth = qSpace.MapToDicGidToSth(
                    (idPts, dic) =>
                    {
                        switch (idPts)
                        {
                            case CharacterBody characterBody:
                                var characterBodyBodySize = characterBody.BodySize;
                                if (dic.SizeToEdge.TryGetValue(characterBodyBodySize, out var walkBlock))
                                {
                                    ITwoDTwoP? pushOutToPt =
                                        walkBlock.PushOutToPt(characterBody.LastPos, characterBody.NowPos);
                                    if (pushOutToPt != null)
                                    {
                                        characterBody.HitWall();
                                    }

                                    return pushOutToPt;
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(idPts));
                        }

                        return null;
                    }, _walkMap);


                qSpace.MoveIdPoint(mapToDicGidToSth!, TempConfig.QSpaceBodyMaxPerLevel);
            }
        }


        public Dictionary<int, HashSet<Bullet>> BulletsDo()
        {
            var whoHitGid = new Dictionary<int, HashSet<Bullet>>();
            foreach (var ii in TeamToBullet)
            {
                var team = ii.Key;
                var bullets = ii.Value;
                var ofType = bullets.OfType<Bullet>();
                foreach (var bullet in ofType)
                {
                    if (!bullet.CanGoATick())
                    {
                        bullets.Remove(bullet);
                        continue;
                    }

                    switch (bullet.TargetType)
                    {
                        case ObjType.OtherTeam:
                            foreach (var i in from kvv in TeamToBodies
                                let bodyTeam = kvv.Key
                                let value = kvv.Value
                                where team != bodyTeam
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
                                    whoHitGid[i] = new HashSet<Bullet> {bullet};
                                }
                            }

                            break;
                        case ObjType.SameTeam:
                            if (TeamToBodies.TryGetValue(team, out var qSpace))
                            {
                                bullet.HitTeam(qSpace);
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
                                    whoHitGid[i] = new HashSet<Bullet> {bullet};
                                }
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return whoHitGid;
        }

        private Dictionary<int, ITwoDTwoP> EveryBodyGoATick(Dictionary<int, Operate> gidToOperates)
        {
            var sepOperatesToTeam = SepOperatesToTeam(gidToOperates);

            var twoDTwoPs = new Dictionary<int, ITwoDTwoP>();
            foreach (var tq in TeamToBodies)
            {
                var team = tq.Key;
                var qSpace = tq.Value;
                if (!sepOperatesToTeam.TryGetValue(team, out var gidToOp)) continue;
                var mapToDicGidToSth = qSpace.MapToDicGidToSth(GameTools.BodyGoATick, gidToOp);

                foreach (var gtb in mapToDicGidToSth)
                {
                    // (gid, (twoDTwoP, bullet))
                    var gid = gtb.Key;
                    var twoDTwoP = gtb.Value.Item1;
                    var bullet = gtb.Value.Item2;
                    if (twoDTwoP != null)
                    {
                        twoDTwoPs[gid] = twoDTwoP;
                    }

                    if (bullet == null) continue;
                    if (TeamToBullet.TryGetValue(team, out var b))
                    {
                        b.Add(bullet);
                    }
                    else
                    {
                        TeamToBullet[team] = new List<IHitStuff> {bullet};
                    }
                }
            }

            return twoDTwoPs;
        }
    }


    public class StartPts
    {
        private int Now;
        private List<TwoDPoint> Pts;

        public StartPts(List<TwoDPoint> pts)
        {
            Pts = pts;
            Now = 0;
        }

        public TwoDPoint GenPt()
        {
            var twoDPoint = Pts[Now % Pts.Count];
            Now += 1;
            return twoDPoint;
        }
    }
}