using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class Skill
    {
        private uint _nowOnTick;

        public int NowTough;

        public bool IsHit;
        private readonly int _baseTough;

        private readonly Dictionary<uint, Bullet> _launchTickToBullet;
        private readonly TwoDVector[] _moves;
        private readonly uint _moveStartTick;

        private bool _isHoming;
        private readonly uint _homingStartTick;
        private readonly uint _homingEndTick;

        private readonly uint _skillMustTick; //必须播放帧
        private readonly uint _comboInputStartTick; //可接受输入操作帧
        private readonly uint _skillMaxTick; // 至多帧，在播放帧
        private readonly WeaponSkillStatus _nextComboHit;
        private readonly WeaponSkillStatus _nextComboMiss;

        public Skill(Dictionary<uint, Bullet> launchTickToBullet, TwoDVector[] moves,
            uint moveStartTick, uint homingStartTick, uint homingEndTick, uint skillMustTick, uint skillMaxTick,
            int baseTough,
            WeaponSkillStatus nextComboHit, uint comboInputStartTick, WeaponSkillStatus nextComboMiss)
        {
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
            IsHit = false;
            _isHoming = false;
        }

        public enum SkillPeriod
        {
            Casting,
            CanCombo,
            End
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

        public (TwoDVector? move, Bullet? launchBullet) GoATick(
            TwoDPoint casterPos, TwoDVector casterAim,
            TwoDPoint? objPos)
        {
            // GenVector
            var lockDistance = objPos == null ? null : casterPos.GenVector(objPos);
            TwoDVector? twoDVector = null;
            if
            (lockDistance != null && _isHoming &&
             _nowOnTick >= _homingStartTick && _nowOnTick < _homingEndTick)
            {
                var rest = _homingEndTick - _nowOnTick;

                twoDVector = lockDistance.Multi(1f / rest);
            }

            else if (_nowOnTick >= _moveStartTick && _nowOnTick < _moveStartTick + _moves.Length)

            {
                var moveStartTick = _nowOnTick - _moveStartTick;

                twoDVector = _moves[moveStartTick];
            }

            // GenBullet 生成子弹
            Bullet? bullet = null;
            if (_launchTickToBullet.TryGetValue(_nowOnTick, out var nowBullet))
            {
                bullet = nowBullet.ActiveBullet(casterPos, casterAim);
            }

            //GONext

            NowTough += TempConfig.ToughGrowPerTick;
            _nowOnTick += 1;


            return (twoDVector, bullet);
        }

        public void LaunchSkill(bool haveLock)
        {
            _isHoming = haveLock;
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