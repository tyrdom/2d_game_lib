using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public enum SkillPeriod
    {
        Casting,
        CanCombo,
        End
    }

    public class Skill : ICharAct
    {
        private LockArea? LockArea { get; }
        public uint NowOnTick { get; set; }
        public int NowTough { get; set; }

        private int SnipeStepNeed { get; }
        private int AmmoCost { get; }

        private int BaseTough { get; }

        public int CanInputMove { get; }
        private Dictionary<uint, Bullet> LaunchTickToBullet { get; }
        private TwoDVector[] Moves { get; }


        private uint MoveStartTick { get; }

        private uint SkillMustTick { get; } //必须播放帧，播放完才能进行下一个技能
        private uint ComboInputStartTick { get; } //可接受输入操作帧

        public uint TotalTick { get; }
        // 至多帧，在播放帧

        private int NextCombo { get; }

        private uint SnipeBreakTick { get; }

        public string LogUser()
        {
            return LaunchTickToBullet.Select(keyValuePair => keyValuePair.Value.Caster)
                .Select(characterStatus =>
                    characterStatus?.GetId().ToString() == null ? "!null!" : characterStatus.GetId().ToString())
                .Aggregate("", (current, @null) => current + @null);
        }

        private Skill(Dictionary<uint, Bullet> launchTickToBullet, TwoDVector[] moves,
            uint moveStartTick, uint skillMustTick, uint totalTick,
            int baseTough, uint comboInputStartTick, int nextCombo,
            LockArea? lockArea, uint snipeBreakTick, int snipeStepNeed, int ammoCost, int canInputMove)
        {
            var b1 = 0 < comboInputStartTick;
            var b2 = skillMustTick < totalTick;
            var b3 = moveStartTick + moves.Length < totalTick;
            var b = b1 &&
                    b2 &&
                    b3;
            if (!b)
            {
#if DEBUG
                Console.Out.WriteLine(
                    $"some skill config is error~~~~~~~reason b1 {b1}  totalTime {b2}  move over {b3}");
#endif
            }

            NowOnTick = 0;
            NowTough = baseTough;
            LaunchTickToBullet = launchTickToBullet;
            Moves = moves;
            MoveStartTick = moveStartTick;
            SkillMustTick = skillMustTick;
            TotalTick = totalTick;
            BaseTough = baseTough;
            ComboInputStartTick = comboInputStartTick;
            NextCombo = nextCombo;
            LockArea = lockArea;
            SnipeBreakTick = snipeBreakTick;
            SnipeStepNeed = snipeStepNeed;
            AmmoCost = ammoCost;
            CanInputMove = canInputMove;
        }


        public void PickedBySomeOne(IBattleUnitStatus characterStatus)
        {
            LockArea?.Sign(characterStatus);
            foreach (var bullet in LaunchTickToBullet.Select(keyValuePair => keyValuePair.Value))
            {
                bullet.Sign(characterStatus);
            }
        }

        public static Skill GenSkillById(string id)
        {
            if (LocalConfig.Configs.skills.TryGetValue(id, out var configsSkill))
            {
                return GenSkillByConfig(configsSkill);
            }

            throw new InvalidDataException($"not such skill id{id}");
        }

        private static Skill GenSkillByConfig(skill skill)
        {
            var twoDVectors = skill.Moves.Select(GameTools.GenVectorByConfig).ToArray();

            var dictionary = skill.LaunchTimeToBullet.ToDictionary(pair => CommonConfig.GetTickByTime(pair.Key),
                pair =>
                {
                    var pairValue = pair.Value;
                    var immutableDictionary = LocalConfig.Configs.bullets;
                    var bullet = immutableDictionary[pairValue];
                    var genByConfig = Bullet.GenByConfig(bullet, CommonConfig.GetTickByTime(pair.Key));
                    return genByConfig;
                });

            var configsLockAreas = LocalConfig.Configs.lock_areas;

            var byConfig = configsLockAreas.TryGetValue(skill.LockArea, out var lockArea)
                ? LockArea.GenByConfig(lockArea)
                : null;
            var skillBaseTough = skill.BaseTough == 0
                ? (int) dictionary.Keys.Min()
                : skill.BaseTough;
            return new Skill(dictionary, twoDVectors, CommonConfig.GetTickByTime(skill.MoveStartTime),
                CommonConfig.GetTickByTime(skill.SkillMustTime), CommonConfig.GetTickByTime(skill.SkillMaxTime),
                skillBaseTough, CommonConfig.GetTickByTime(skill.ComboInputStartTime), skill.NextCombo, byConfig,
                CommonConfig.GetTickByTime(skill.BreakSnipeTime),
                skill.SnipeStepNeed, skill.AmmoCost, CommonConfig.GetIntTickByTime(skill.CanInputMove));
        }


        public SkillPeriod InWhichPeriod()
        {
            if (NowOnTick < SkillMustTick)
            {
                return SkillPeriod.Casting;
            }

            return NowOnTick < TotalTick ? SkillPeriod.CanCombo : SkillPeriod.End;
        }

        public int? ComboInputRes() //可以连击，返回 下一个动作
        {
            // 如果命中返回命中连击状态id，如果不是返回miss连击id，大部分是一样的
            var weaponSkillStatus = NextCombo;
            return NowOnTick >= ComboInputStartTick && NowOnTick < TotalTick
                ? weaponSkillStatus
                : (int?) null;
        }

        public (ITwoDTwoP? move, IEffectMedia? bullet, bool snipeOff, ICanPutInMapInteractable? getFromCage, MapInteract
            interactive) GoATick(TwoDPoint casterPos,
                TwoDVector casterAim,
                TwoDVector? rawMoveVector, TwoDVector? limitV)
        {
            NowOnTick++;
            // 生成攻击运动
            TwoDVector? move = null;
            if (NowOnTick < CanInputMove)
            {
                move = rawMoveVector;
            }
            else if (NowOnTick >= MoveStartTick && NowOnTick < MoveStartTick + Moves.Length)
            {
                var moveOnTick = NowOnTick - MoveStartTick;

                move = Moves[moveOnTick];
                if (limitV != null)
                {
                    var movesLengthRest = (float) Moves.Length - moveOnTick;
                    var min = MathTools.Min(limitV.X, move.X);
                    var max = MathTools.Max(limitV.Y, move.Y) / movesLengthRest;

                    move = new TwoDVector(min, max);
                }

                move = move.AntiClockwiseTurn(casterAim);
            }


            // GenBullet 生成子弹
            IPosMedia? bullet = null;
            if (NowOnTick == 0 && LockArea != null)
            {
                bullet = LockArea.Active(casterPos, casterAim);
            }

            NowTough += LocalConfig.ToughGrowPerTick;

            if (LaunchTickToBullet.TryGetValue(NowOnTick, out var nowBullet))
            {
                bullet = nowBullet.Active(casterPos, casterAim);
            }

            // 是否退出Snipe状态
            var snipeOff = NowOnTick > 0 && NowOnTick == SnipeBreakTick;

            //GONext

            return (move, bullet, snipeOff, null, MapInteract.PickCall);
        }

        public (IPosMedia? posMedia, bool canInputMove) GetSkillStart(TwoDPoint casterPos, TwoDVector casterAim)
        {
            return (LockArea?.Active(casterPos, casterAim), NowOnTick < CanInputMove);
        }

        public bool Launch(int nowSnipeStep, int nowAmmo)
        {
            if (nowSnipeStep < SnipeStepNeed || nowAmmo < AmmoCost) return false;
            NowTough = BaseTough;
            NowOnTick = 0;
            return true;
        }
    }
}