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
        private readonly LockArea? _lockArea;
        public uint NowOnTick { get; set; }
        public int NowTough { get; set; }

        private int SnipeStepNeed { get; }
        private int AmmoCost { get; }

        private int BaseTough { get; }

        public bool CanInputMove { get; }
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
                    characterStatus?.GId.ToString() == null ? "!null!" : characterStatus.GId.ToString())
                .Aggregate("", (current, @null) => current + @null);
        }

        private Skill(Dictionary<uint, Bullet> launchTickToBullet, TwoDVector[] moves,
            uint moveStartTick, uint skillMustTick, uint totalTick,
            int baseTough, uint comboInputStartTick, int nextCombo,
            LockArea? lockArea, uint snipeBreakTick, int snipeStepNeed, int ammoCost, bool canInputMove)
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
            _lockArea = lockArea;
            SnipeBreakTick = snipeBreakTick;
            SnipeStepNeed = snipeStepNeed;
            AmmoCost = ammoCost;
            CanInputMove = canInputMove;
        }


        public void PickedBySomeOne(CharacterStatus characterStatus)
        {
            if (_lockArea != null) _lockArea.Caster = characterStatus;
            foreach (var bullet in LaunchTickToBullet.Select(keyValuePair => keyValuePair.Value))
            {
                bullet.PickedBySomeOne(characterStatus);
            }
        }

        public static Skill GenSkillById(string id)
        {
            if (TempConfig.Configs.skills.TryGetValue(id, out var configsSkill))
            {
                return GenSkillByConfig(configsSkill);
            }

            throw new InvalidDataException($"not such skill id{id}");
        }

        private static Skill GenSkillByConfig(skill skill)
        {
            var twoDVectors = skill.Moves.Select(GameTools.GenVectorByConfig).ToArray();

            var dictionary = skill.LaunchTimeToBullet.ToDictionary(pair => TempConfig.GetTickByTime(pair.Key),
                pair =>
                {
                    var pairValue = pair.Value;
                    var immutableDictionary = TempConfig.Configs.bullets;
                    var bullet = immutableDictionary[pairValue];
                    var genByConfig = Bullet.GenByConfig(bullet, TempConfig.GetTickByTime(pair.Key));
                    return genByConfig;
                });

            var configsLockAreas = TempConfig.Configs.lock_areas;

            var byConfig = configsLockAreas.TryGetValue(skill.LockArea, out var lockArea)
                ? LockArea.GenByConfig(lockArea)
                : null;
            return new Skill(dictionary, twoDVectors, TempConfig.GetTickByTime(skill.MoveStartTime),
                TempConfig.GetTickByTime(skill.SkillMustTime), TempConfig.GetTickByTime(skill.SkillMaxTime),
                skill.BaseTough, TempConfig.GetTickByTime(skill.ComboInputStartTime), skill.NextCombo, byConfig,
                TempConfig.GetTickByTime(skill.BreakSnipeTime),
                skill.SnipeStepNeed, skill.AmmoCost, skill.CanInputMove);
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

        public (ITwoDTwoP? move, IHitMedia? bullet, bool snipeOff, ICanPutInMapInteractable? getFromCage, MapInteract
            interactive) GoATick(TwoDPoint casterPos,
                TwoDVector casterAim,
                TwoDVector? rawMoveVector, TwoDVector? limitV)
        {
            // 生成攻击运动
            TwoDVector? move = null;
            if (NowOnTick >= MoveStartTick)
            {
                if (CanInputMove)
                {
                    move = rawMoveVector;
                }
                else if (NowOnTick < MoveStartTick + Moves.Length)
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
            }

            // GenBullet 生成子弹
            IHitMedia? bullet = null;
            if (NowOnTick == 0 && _lockArea != null)
            {
                bullet = _lockArea.ActiveArea(casterPos, casterAim);
            }

            if (LaunchTickToBullet.TryGetValue(NowOnTick, out var nowBullet))
            {
                bullet = nowBullet.ActiveBullet(casterPos, casterAim);
            }

            // 是否退出Snipe状态
            var snipeOff = NowOnTick > 0 && NowOnTick == SnipeBreakTick;

            //GONext
            NowTough += TempConfig.ToughGrowPerTick;
            NowOnTick++;
            return (move, bullet, snipeOff, null, MapInteract.PickCall);
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