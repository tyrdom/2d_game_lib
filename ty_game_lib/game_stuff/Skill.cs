﻿using System;
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

        public int GetIntId()
        {
            return (int) SkillId;
        }

        public skill_id SkillId { get; }
        public int NowTough { get; set; }

        public int SnipeStepNeed { get; }
        public int AmmoCost { get; }

        private int BaseTough { get; }

        private int CanInputMove { get; }
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
            LockArea? lockArea, uint snipeBreakTick, int snipeStepNeed, int ammoCost, int canInputMove,
            skill_id skillId)
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
            SkillId = skillId;
        }


        public void PickedBySomeOne(IBattleUnitStatus characterStatus)
        {
            LockArea?.Sign(characterStatus);
            foreach (var bullet in LaunchTickToBullet.Values)
            {
                bullet.Sign(characterStatus);
            }
        }

        public static Skill GenSkillById(string id)
        {
            var o = (skill_id) Enum.Parse(typeof(skill_id), id, true);
            return GenSkillById(o);
        }

        public static Skill GenSkillById(skill_id id)
        {
            if (CommonConfig.Configs.skills.TryGetValue(id, out var configsSkill))
            {
                return GenSkillByConfig(configsSkill);
            }

            throw new InvalidDataException($"not such skill id{id}");
        }

        private static Skill GenSkillByConfig(skill skill)
        {
            var twoDVectors = skill.Moves.Select(GameTools.GenVectorByConfig).ToArray();

            var dictionary = skill.LaunchTimeToBullet.ToDictionary(pair => pair.Key,
                pair =>
                {
                    var pairValue = pair.Value;
                    var genByConfig = Bullet.GenById(pairValue, pair.Key);
                    return genByConfig;
                });

            var byConfig = LockArea.TryGenById(skill.LockArea, out var area)
                ? area
                : null;
            var skillBaseTough = skill.BaseTough == 0
                ? (int) dictionary.Keys.Min()
                : skill.BaseTough;
            return new Skill(dictionary, twoDVectors, skill.MoveStartTime,
                skill.SkillMustTime, skill.SkillMaxTime,
                skillBaseTough, skill.ComboInputStartTime, skill.NextCombo, byConfig,
                skill.BreakSnipeTime,
                skill.SnipeStepNeed - 1, skill.AmmoCost, (int) skill.CanInputMove, skill.id);
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
            // 返回连击id
            return NowOnTick >= ComboInputStartTick && NowOnTick < TotalTick
                ? NextCombo
                : (int?) null;
        }

        public action_type GetTypeEnum()
        {
            return action_type.skill;
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
                    var movesLengthRest = (float) (Moves.Length - moveOnTick);
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

            NowTough += CommonConfig.OtherConfig.tough_grow;

            if (LaunchTickToBullet.TryGetValue(NowOnTick, out var nowBullet))
            {
#if DEBUG
                Console.Out.WriteLine(
                    $"to launch a bullet ~~~ {LaunchTickToBullet.Keys.Aggregate("", ((s, u) => s + "." + u))}");
#endif
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
            if (nowSnipeStep < SnipeStepNeed || nowAmmo < AmmoCost)
            {
#if DEBUG
                Console.Out.WriteLine(
                    $"not enough ammo {AmmoCost} / {nowAmmo} or not enough step {SnipeStepNeed}/ {nowSnipeStep}");
#endif
                return false;
            }

            NowTough = BaseTough;
            NowOnTick = 0;
            return true;
        }
    }
}