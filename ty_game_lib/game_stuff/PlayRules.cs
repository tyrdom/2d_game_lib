using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace game_stuff
{
    public interface IPlayRuler
    {
        IRuleTickResult RulerGoTick(PlayGround playGround);
        LevelUps GetLevelUp();
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


    public class RebornUnit
    {
        public RebornUnit(CharacterBody characterBody, int rebornTickRest, GameItem[] fastRebornCost)
        {
            CharacterBody = characterBody;
            RebornTickRest = rebornTickRest;
            FastRebornCost = fastRebornCost;
        }

        public GameItem[] FastRebornCost { get; }

        public static int GenRebornTick(CharacterBody characterBody)
        {
            return characterBody.GetAutoRebornTick().ReBornAboutTick;
        }

        public static GameItem[] GetFastRebItems(CharacterBody characterBody)
        {
            return characterBody.GetAutoRebornTick().RebornCost;
        }

        public void SetRebornTime()
        {
            RebornTickRest = GenRebornTick(CharacterBody);
        }

        public CharacterBody CharacterBody { get; }
        private int RebornTickRest { get; set; }


        public void GoATick()
        {
            if (RebornTickRest <= 0) return;
            RebornTickRest--;
        }

        public bool RebornAboutTickFinish()
        {
            var canReborn = RebornTickRest == 0;
            RebornTickRest = -1;
            return canReborn;
        }
    }
}