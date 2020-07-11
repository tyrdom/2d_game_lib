﻿using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class Skill
    {
        private LockArea? _lockArea;
        private uint _nowOnTick;

        public int NowTough;

        public bool IsHit;
        private readonly int _baseTough;

        private readonly Dictionary<uint, Bullet> _launchTickToBullet;
        private readonly TwoDVector[] _moves;
        private readonly uint _moveStartTick;

        private readonly uint _homingStartTick; //可操作帧开始
        private readonly uint _homingEndTick; //可操作帧结束

        private readonly uint _skillMustTick; //必须播放帧，播放完才能进行下一个技能
        private readonly uint _comboInputStartTick; //可接受输入操作帧
        private readonly uint _skillMaxTick; // 至多帧，在播放帧
        private readonly WeaponSkillStatus _nextComboHit;
        private readonly WeaponSkillStatus _nextComboMiss;

        public Skill(Dictionary<uint, Bullet> launchTickToBullet, TwoDVector[] moves,
            uint moveStartTick, uint homingStartTick, uint homingEndTick, uint skillMustTick, uint skillMaxTick,
            int baseTough,
            WeaponSkillStatus nextComboHit, uint comboInputStartTick, WeaponSkillStatus nextComboMiss,
            LockArea? lockArea)
        {
            var b = homingStartTick < homingEndTick && homingEndTick < comboInputStartTick &&
                    skillMustTick < skillMaxTick &&
                    moveStartTick + moves.Length < skillMaxTick;
            if (!b)
            {
                Console.Out.WriteLine("some skill config is error~~~~~~~");
            }

            _nowOnTick = 0;
            NowTough = baseTough;
            _launchTickToBullet = launchTickToBullet;
            _moves = moves;
            _moveStartTick = moveStartTick;
            _homingStartTick = homingStartTick;
            _homingEndTick = homingEndTick;
            _skillMustTick = skillMustTick;
            _skillMaxTick = skillMaxTick;
            _baseTough = baseTough;
            _nextComboHit = nextComboHit;
            _comboInputStartTick = comboInputStartTick;
            _nextComboMiss = nextComboMiss;
            _lockArea = lockArea;
            IsHit = false;
        }

        public enum SkillPeriod
        {
            Casting,
            CanCombo,
            End
        }

        public bool CanChangeAim()
        {
            return _nowOnTick < _homingEndTick && _nowOnTick >= _homingStartTick;
        }

        public SkillPeriod InWhichPeriod()
        {
            if (_nowOnTick < _skillMustTick)
            {
                return SkillPeriod.Casting;
            }

            return _nowOnTick < _skillMaxTick ? SkillPeriod.CanCombo : SkillPeriod.End;
        }

        public WeaponSkillStatus? ComboInputRes()
        {
            var weaponSkillStatus = IsHit ? _nextComboHit : _nextComboMiss;
            return _nowOnTick >= _comboInputStartTick && _nowOnTick < _skillMaxTick
                ? weaponSkillStatus
                : (WeaponSkillStatus?) null;
        }

        public (TwoDVector? move, IHitStuff? launchBullet) GoATick(
            TwoDPoint casterPos, TwoDVector casterAim, TwoDVector? approachingVector)
        {
            // 生成攻击运动
            TwoDVector? twoDVector = null;

            if (_nowOnTick >= _moveStartTick && _nowOnTick < _moveStartTick + _moves.Length)
            {
                var moveStartTick = _nowOnTick - _moveStartTick;

                twoDVector = _moves[moveStartTick];
                if (approachingVector != null)
                {
                    var min = MathTools.Min(approachingVector.X, twoDVector.X);
                    var max = MathTools.Max(approachingVector.Y, twoDVector.Y);
                    twoDVector.X = min;
                    twoDVector.Y = max;
                }

                twoDVector = twoDVector.AntiClockwiseTurn(casterAim);
            }

            // GenBullet 生成子弹
            IHitStuff? bullet = null;
            if (_nowOnTick == 0 && _lockArea != null)
            {
                bullet = _lockArea;
            }


            else if (_launchTickToBullet.TryGetValue(_nowOnTick, out var nowBullet))
            {
                bullet = nowBullet.ActiveBullet(casterPos, casterAim);
            }

            //GONext

            NowTough += TempConfig.ToughGrowPerTick;
            _nowOnTick += 1;


            return (twoDVector, bullet);
        }

        public void LaunchSkill()
        {
            IsHit = false;
            NowTough = _baseTough;
            _nowOnTick = 0;
        }

        // public static Skill InitFormConfig(game_config.Skill skillConfig)
        // {
        //     
        // }
    }
}