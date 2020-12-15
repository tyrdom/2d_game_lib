using System;
using System.Collections.Generic;
using System.Linq;

namespace game_stuff
{
    public interface IPlayRules
    {
        IGroundResult CheckFinish(PlayGround playGround);
    }

    public readonly struct PveResult : IGroundResult
    {
        public PveResult(PveResultType pveResultType)
        {
            PveResultType = pveResultType;
        }

        public PveResultType PveResultType { get; }
    }

    public readonly struct PvPResult : IGroundResult
    {
        public int WinTeam { get; }

        public bool IsFinish { get; }

        public Dictionary<int, HashSet<int>> GidToKillsBroadcast { get; }

        public PvPResult(int winTeam, bool isFinish, Dictionary<int, HashSet<int>> gidToKillBroadcast)
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
        private int ScoreReach { get; }

        public IGroundResult CheckFinish(PlayGround playGround)
        {
            var checkKills = playGround.CheckKills();
            return new PvPResult();
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

        public IGroundResult CheckFinish(PlayGround playGround)
        {
            var pveResultType = playGround.CheckPve(PveWinCond);
            return new PveResult(pveResultType);
        }
    }
}