using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_stuff;

namespace rogue_game
{
    public class PveMap
    {
        public PveMap(PlayGround playGround, HashSet<CharacterBody> bosses, HashSet<CharacterBody> creeps,
            PveWinCond pveWinCond, TwoDPoint[] enterPoints)
        {
            PlayGround = playGround;
            Bosses = bosses;
            Creeps = creeps;
            PveWinCond = pveWinCond;
            EnterPoints = enterPoints;
        }

        private PlayGround PlayGround { get; }
        public HashSet<CharacterBody> Bosses { get; }
        public HashSet<CharacterBody> Creeps { get; }
        public PveWinCond PveWinCond { get; }

        public TwoDPoint[] EnterPoints { get; }

        public void ActiveApplyDevice()
        {
            PlayGround.ActiveApplyDevice();
        }

        public bool IsClear()
        {
            return PveWinCond switch
            {
                PveWinCond.AllClear => Bosses.All(x => x.CharacterStatus.SurvivalStatus.IsDead()) &&
                                       Creeps.All(x => x.CharacterStatus.SurvivalStatus.IsDead()),
                PveWinCond.BossClear => Bosses.All(x => x.CharacterStatus.SurvivalStatus.IsDead()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public ((Dictionary<int, HashSet<HitResult>> playerBehit, Dictionary<int, HashSet<HitResult>> trapBehit)
            gidToWhichBulletHit, Dictionary<int, IEnumerable<ISeeTickMsg>> playerSeeMsg) PlayGroundGoATick(
                Dictionary<int, Operate> valuePairs)
        {
            return PlayGround.PlayGroundGoATick(valuePairs);
        }

        public void AddPlayers(CharacterBody[] characterBodies)
        {
            var valueTuples = new List<(CharacterBody, TwoDPoint)>();
            for (var i = 0; i < characterBodies.Length; i++)
            {
                var twoDPoint = this.EnterPoints[i % EnterPoints.Length];
                valueTuples.Add((characterBodies[i], twoDPoint));
            }

            PlayGround.AddBodies(valueTuples);
        }
    }

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

        public void GetStart(CharacterBody[] characterBodies)
        {
            Entrance.AddPlayers(characterBodies);
        }

        public bool IsPass()
        {
            return Finish.IsClear();
        }
    }
}