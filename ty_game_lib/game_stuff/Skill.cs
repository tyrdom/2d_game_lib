using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class Skill
    {
        private int _nowOnTick;

        private int _nowTough;

        private readonly int _baseTough;

        private readonly Dictionary<int, Bullet> _launchTickToBullet;
        private readonly TwoDVector[] _moves;
        private readonly int _moveStartTick;

        private bool _isHoming;
        private readonly int _homingStartTick;
        private readonly int _homingEndTick;

        private readonly int _skillMustTick; //必须播放帧
        private readonly int _comboInputStartTick; //可接受输入操作帧
        private readonly int _skillMaxTick; // 至多帧，在播放帧
        public readonly WeaponSkillStatus NextCombo; 


        public Skill(int nowOnTick, int nowTough, Dictionary<int, Bullet> launchTickToBullet, TwoDVector[] moves,
            int moveStartTick, int homingStartTick, int homingEndTick, int skillMustTick, int skillMaxTick, int baseTough,
            WeaponSkillStatus nextCombo, int comboInputStartTick)
        {
            _nowOnTick = nowOnTick;
            _nowTough = nowTough;
            _launchTickToBullet = launchTickToBullet;
            _moves = moves;
            _moveStartTick = moveStartTick;
            _homingStartTick = homingStartTick;
            _homingEndTick = homingEndTick;
            _skillMustTick = skillMustTick;
            _skillMaxTick = skillMaxTick;
            _baseTough = baseTough;
            NextCombo = nextCombo;
            _comboInputStartTick = comboInputStartTick;
            _isHoming = false;
        }

        public (TwoDVector? move, Bullet? lauchBullet, bool isEnd) GoATick(TwoDPoint casterPos, TwoDVector casterAim,
            CharacterStatus caster,
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
                bullet = nowBullet.ActiveBullet(casterPos, casterAim, caster, _nowTough);
            }

            //GONext
            if (_nowOnTick < _skillMaxTick)
            {
                _nowTough += TempConfig.ToughGrowPerTick;
                _nowOnTick += 1;

                return (twoDVector, bullet, false);
            }

            return (twoDVector, bullet, true);
        }

        public Skill LaunchSkill(bool haveLock)
        {
            _isHoming = haveLock;
            _nowTough = _baseTough;
            return this;
        }

        
    }
}