using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace game_stuff
{
    public interface IPlayRules
    {
        IRuleTickResult CheckFinish(PlayGround playGround);
    }

    public readonly struct PveResult : IRuleTickResult
    {
        public PveResult(PveResultType pveResultType)
        {
            PveResultType = pveResultType;
        }

        public PveResultType PveResultType { get; }
    }

    public readonly struct PvPResult : IRuleTickResult
    {
        public bool IsFinish { get; }
        public int? WinTeam { get; }


        public Dictionary<int, ImmutableHashSet<int>> GidToKillsBroadcast { get; }

        public PvPResult(int? winTeam, bool isFinish, Dictionary<int, ImmutableHashSet<int>> gidToKillBroadcast)
        {
            WinTeam = winTeam;
            IsFinish = isFinish;
            GidToKillsBroadcast = gidToKillBroadcast;
        }
    }

    public enum PveResultType
    {
        PveWin,
        PveLoss,
        NotFinish
    }

    public enum PveWinCond

    {
        AllClear,
        BossClear
    }


    public class PVPKillScoreRules : IPlayRules
    {
        public PVPKillScoreRules(int scoreReach)
        {
            ScoreReach = scoreReach;
            TeamScores = new Dictionary<int, int>();
        }

        private int ScoreReach { get; }
        private Dictionary<int, int> TeamScores { get; }

        public IRuleTickResult CheckFinish(PlayGround playGround)
        {
            var playGroundGidToBody = playGround.GidToBody;
            var valueTuples = playGroundGidToBody
                .Select(x => (x.Value.Team, x.Value.CharacterStatus.CharRuleData.NowKills, x.Key))
                .Where(xx => xx.NowKills.Any());
            var enumerable = valueTuples.ToList();
            var dictionary = enumerable.ToDictionary(k => k.Key,
                k => k.NowKills.Select(dd => dd.GetId()).ToList().ToImmutableHashSet());
            foreach (var valueTuple in enumerable.GroupBy(x => x.Team))
            {
                var sum = valueTuple.Sum(x => x.NowKills.Count);
                var valueTupleKey = valueTuple.Key;
                if (TeamScores.TryGetValue(valueTupleKey, out var score))
                {
                    var teamScore = score + sum;
                    if (teamScore >= ScoreReach)
                    {
                        return new PvPResult(valueTupleKey, true, dictionary);
                    }

                    TeamScores[valueTupleKey] = teamScore;
                }
                else
                {
                    if (sum >= ScoreReach)
                    {
                        return new PvPResult(valueTupleKey, true, dictionary);
                    }

                    TeamScores[valueTupleKey] = sum;
                }
            }

            return new PvPResult(null, false, dictionary);
            //todo push select
        }
    }

    public class PVERules : IPlayRules
    {
        public PVERules(PveWinCond pveWinCond, GameItem winBonus, IMapInteractable winSpawn, int playerInTeam)
        {
            PveWinCond = pveWinCond;
            WinBonus = winBonus;
            WinSpawn = winSpawn;
            PlayerInTeam = playerInTeam;
        }

        public int PlayerInTeam { get; }
        public PveWinCond PveWinCond { get; }

        public GameItem WinBonus { get; }

        public IMapInteractable WinSpawn { get; }

        public IRuleTickResult CheckFinish(PlayGround playGround)
        {
            var pveResultType = playGround.CheckPve(PveWinCond);
            return new PveResult(pveResultType);
        }
    }
}