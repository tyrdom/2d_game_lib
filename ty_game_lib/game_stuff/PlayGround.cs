using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class PlayGround
    {
        private Dictionary<int, QSpace> TeamToBodies;
        private readonly SightMap SightMap;
        private readonly WalkMap WalkMap;
        private Dictionary<int, CharacterBody> GidToBody;
        private Dictionary<int, List<Bullet>> TeamToBullet;

        public PlayGround(Dictionary<int, QSpace> teamToBodies, SightMap sightMap, WalkMap walkMap,
            Dictionary<int, CharacterBody> gidToBody, Dictionary<int, List<Bullet>> teamToBullet)
        {
            TeamToBodies = teamToBodies;
            SightMap = sightMap;
            WalkMap = walkMap;
            GidToBody = gidToBody;
            TeamToBullet = teamToBullet;
        }

        public static (PlayGround, Dictionary<int, HashSet<CharInitMsg>>) InitPlayGround(
            IEnumerable<PlayerInitData> playerInitData, MapInitData mapInitData)
        {
            var qSpaces = new Dictionary<int, HashSet<CharacterBody>>();
            var characterBodies = new Dictionary<int, CharacterBody>();

            foreach (var initData in playerInitData)
            {
                var initDataTeamId = initData.TeamId;
                if (mapInitData.TeamToStartPt.TryGetValue(initDataTeamId, out var startPts))
                {
                    var twoDPoint = startPts.GenPt();
                    var genCharacterBody = initData.GenCharacterBody(twoDPoint);
                    if (qSpaces.TryGetValue(initDataTeamId, out var qSpace))
                    {
                        qSpace.Add(genCharacterBody);
                    }
                    else
                    {
                        qSpaces[initDataTeamId] = new HashSet<CharacterBody> {genCharacterBody};
                    }

                    characterBodies[initData.Gid] = genCharacterBody;
                }
            }

            var spaces = qSpaces.ToDictionary(p => p.Key, p =>
            {
                var hashSet = p.Value;
                var zone = mapInitData.GetZone();
                var emptyRootBranch = SomeTools.CreateEmptyRootBranch(zone);
                var aabbBoxShapes = hashSet.Select(x => x.CovToAabbPackBox()).ToHashSet();
                emptyRootBranch.AddIdPoint(aabbBoxShapes, TempConfig.QSpaceBodyMaxPerLevel);
                return emptyRootBranch;
            });

            var playGround = new PlayGround(spaces, mapInitData.SightMap, mapInitData.WalkMap, characterBodies,
                new Dictionary<int, List<Bullet>>());
            return (playGround, playGround.GenInitMsg());
        }

        Dictionary<int, HashSet<CharInitMsg>> GenInitMsg()
        {
            var dictionary = new Dictionary<int, HashSet<CharInitMsg>>();
            var charInitMsgs = GidToBody.ToDictionary(p => p.Key, p => p.Value.GenInitMsg());
            foreach (var (key, value) in charInitMsgs)
            {
                if (dictionary.TryGetValue(key, out var charInitMsglist))
                {
                    charInitMsglist.Add(value);
                }
                else
                {
                    dictionary[key] = new HashSet<CharInitMsg> {value};
                }
            }

            return dictionary;
        }

        public Dictionary<int, Dictionary<int, Operate>> SepOperatesToTeam(Dictionary<int, Operate> gidToOperates)
        {
            var dictionary = new Dictionary<int, Dictionary<int, Operate>>();

            foreach (var (gid, operate) in gidToOperates)
            {
                if (GidToBody.TryGetValue(gid, out var characterBody))
                {
                    dictionary[characterBody.Team][gid] = operate;
                }
            }

            return dictionary;
        }

        public (Dictionary<int, IEnumerable<BulletMsg>>, Dictionary<int, IEnumerable<CharTickMsg>>
            ) PlayGroundGoATick(Dictionary<int, Operate> gidToOperates)
        {
            var everyBodyGoATick = EveryBodyGoATick(gidToOperates);
            var gidToWhichBulletHit = BulletsDo();
            foreach (var (_, qSpace) in TeamToBodies)
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
            foreach (var (key, characterBody) in GidToBody)
            {
                var characterBodies = new HashSet<CharacterBody>();
                foreach (var (bTeam, qSpace) in TeamToBodies)
                {
                    if (characterBody.Team == bTeam)
                    {
                        var filterToGIdPsList = qSpace.FilterToGIdPsList((x, y) => true, true).Select(x =>
                        {
                            return x switch
                            {
                                CharacterBody aCharacterBody => aCharacterBody,
                                _ => throw new ArgumentOutOfRangeException(nameof(x))
                            };
                        });
                        characterBodies.UnionWith(filterToGIdPsList);
                    }
                    else
                    {
                        var filterToGIdPsList =
                            qSpace.FilterToGIdPsList((idp, acb) => { return acb.InSight(idp, SightMap); },
                                characterBody);
                        characterBodies.UnionWith(filterToGIdPsList.Select(x =>
                        {
                            return x switch
                            {
                                CharacterBody characterBody1 => characterBody1,
                                _ => throw new ArgumentOutOfRangeException(nameof(x))
                            };
                        }));
                    }
                }

                gidToCharacterBodies[key] = characterBodies;
            }

            return gidToCharacterBodies;
        }

        public void BodiesRePlace()
        {
            foreach (var (_, qSpace) in TeamToBodies)
            {
                var mapToDicGidToSth = qSpace.MapToDicGidToSth((idpts, dic) =>
                {
                    switch (idpts)
                    {
                        case CharacterBody characterBody:
                            var characterBodyBodySize = characterBody.BodySize;
                            if (dic.SizeToEdge.TryGetValue(characterBodyBodySize, out var walkBlock))
                            {
                                ITwoDTwoP pushOutToPt =
                                    walkBlock.PushOutToPt(characterBody.LastPos, characterBody.NowPos);
                                if (pushOutToPt != null)
                                {
                                    characterBody.HitWall();
                                }

                                return pushOutToPt;
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(idpts));
                    }

                    return null;
                }, WalkMap);
                qSpace.MoveIdPoint(mapToDicGidToSth, TempConfig.QSpaceBodyMaxPerLevel);
            }
        }


        public Dictionary<int, HashSet<Bullet>> BulletsDo()
        {
            var whoHitGid = new Dictionary<int, HashSet<Bullet>>();
            foreach (var (team, bullets) in TeamToBullet)
            {
                foreach (var bullet in bullets)
                {
                    if (!bullet.CanGoATick())
                    {
                        bullets.Remove(bullet);
                        continue;
                    }

                    switch (bullet.ObjType)
                    {
                        case ObjType.OtherTeam:
                            foreach (var (bodyTeam, value) in TeamToBodies)
                            {
                                if (team == bodyTeam) continue;
                                var hitTeam = bullet.HitTeam(value);
                                foreach (var i in hitTeam)
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
                            }

                            break;
                        case ObjType.SameTeam:
                            if (TeamToBodies.TryGetValue(team, out var qSpace))
                            {
                                bullet.HitTeam(qSpace);
                            }

                            break;
                        case ObjType.AllTeam:
                            foreach (var i in from QSpace value in TeamToBodies
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
            foreach (var (team, qSpace) in TeamToBodies)
            {
                if (!sepOperatesToTeam.TryGetValue(team, out var gidToOp)) continue;
                var mapToDicGidToSth = qSpace.MapToDicGidToSth(GameTools.BodyGoATick, gidToOp);

                foreach (var (gid, (twoDTwoP, bullet)) in mapToDicGidToSth)
                {
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
                        TeamToBullet[team] = new List<Bullet> {bullet};
                    }
                }
            }

            return twoDTwoPs;
        }
    }

    public class MapInitData
    {
        public SightMap SightMap;
        public WalkMap WalkMap;
        public Dictionary<int, StartPts> TeamToStartPt;

        public Zone GetZone()
        {
            return (from WalkBlock walkBlock in WalkMap.SizeToEdge select walkBlock.QSpace.Zone).Aggregate(
                SightMap.Lines.Zone, (current, qSpaceZone) => current.Join(qSpaceZone));
        }

        public MapInitData(SightMap sightMap, WalkMap walkMap, Dictionary<int, StartPts> teamToStartPt)
        {
            SightMap = sightMap;
            WalkMap = walkMap;
            TeamToStartPt = teamToStartPt;
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