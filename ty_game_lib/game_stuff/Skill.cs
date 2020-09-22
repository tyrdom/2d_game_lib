using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Skill
    {
        private readonly LockArea? _lockArea;
        public uint NowOnTick;
        public int NowTough;

        public int SnipeStepNeed;
        private readonly int _baseTough;

        private readonly Dictionary<uint, Bullet> _launchTickToBullet;
        private readonly TwoDVector[] _moves;


        private readonly uint _moveStartTick;

        private readonly uint _skillMustTick; //必须播放帧，播放完才能进行下一个技能
        private readonly uint _comboInputStartTick; //可接受输入操作帧
        private readonly uint _skillMaxTick; // 至多帧，在播放帧
        private readonly int _nextCombo;

        private int SnipeBreakTick { get; }

        public string LogUser()
        {
            return _launchTickToBullet.Select(keyValuePair => keyValuePair.Value.Caster)
                .Select(characterStatus =>
                    characterStatus?.GId.ToString() == null ? "!null!" : characterStatus.GId.ToString())
                .Aggregate("", (current, @null) => current + @null);
        }

        private Skill(Dictionary<uint, Bullet> launchTickToBullet, TwoDVector[] moves,
            uint moveStartTick, uint skillMustTick, uint skillMaxTick,
            int baseTough, uint comboInputStartTick, int nextCombo,
            LockArea? lockArea, int snipeBreakTick, int snipeStepNeed)
        {
            var b = 0 < comboInputStartTick &&
                    skillMustTick < skillMaxTick &&
                    moveStartTick + moves.Length < skillMaxTick;
            if (!b)
            {
                Console.Out.WriteLine("some skill config is error~~~~~~~");
            }

            NowOnTick = 0;
            NowTough = baseTough;
            _launchTickToBullet = launchTickToBullet;
            _moves = moves;
            _moveStartTick = moveStartTick;
            _skillMustTick = skillMustTick;
            _skillMaxTick = skillMaxTick;
            _baseTough = baseTough;
            _comboInputStartTick = comboInputStartTick;
            _nextCombo = nextCombo;
            _lockArea = lockArea;
            SnipeBreakTick = snipeBreakTick;
            SnipeStepNeed = snipeStepNeed;
        }


        public void PickedBySomeOne(CharacterStatus characterStatus)
        {
            if (_lockArea != null) _lockArea.Caster = characterStatus;
            foreach (var bullet in _launchTickToBullet.Select(keyValuePair => keyValuePair.Value))
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

        public static Skill GenSkillByConfig(skill skill)
        {
            var twoDVectors = skill.Moves.Select(GameTools.GenVectorByConfig).ToArray();

            var dictionary = skill.LaunchTickToBullet.ToDictionary(pair => pair.Key, pair =>
            {
                var pairValue = pair.Value;
                var immutableDictionary = TempConfig.Configs.bullets;
                var bullet = immutableDictionary[pairValue];
                var genByConfig = Bullet.GenByConfig(bullet, pair.Key);
                return genByConfig;
            });
            var configsLockAreas = TempConfig.Configs.lock_areas;
            var byConfig = configsLockAreas.TryGetValue(skill.LockArea, out var lockArea)
                ? LockArea.GenByConfig(lockArea)
                : null;


            return new Skill(dictionary, twoDVectors, skill.MoveStartTick, skill.SkillMustTick, skill.SkillMaxTick,
                skill.BaseTough, skill.ComboInputStartTick, skill.NextCombo, byConfig, skill.BreakSnipeTick,
                skill.SnipeStepNeed);
        }

        public enum SkillPeriod
        {
            Casting,
            CanCombo,
            End
        }

        public SkillPeriod InWhichPeriod()
        {
            if (NowOnTick < _skillMustTick)
            {
                return SkillPeriod.Casting;
            }

            return NowOnTick < _skillMaxTick ? SkillPeriod.CanCombo : SkillPeriod.End;
        }

        public int? ComboInputRes() //可以连击，返回 下一个动作
        {
            // 如果命中返回命中连击状态id，如果不是返回miss连击id，大部分是一样的
            var weaponSkillStatus = _nextCombo;
            return NowOnTick >= _comboInputStartTick && NowOnTick < _skillMaxTick
                ? weaponSkillStatus
                : (int?) null;
        }

        public (TwoDVector? move, IHitStuff? bullet, bool snipeOff) GoATick(
            TwoDPoint casterPos, TwoDVector casterAim, TwoDVector? approachingVector)
        {
            // 生成攻击运动
            TwoDVector? move = null;

            if (NowOnTick >= _moveStartTick && NowOnTick < _moveStartTick + _moves.Length)
            {
                var moveOnTick = NowOnTick - _moveStartTick;

                move = _moves[moveOnTick];
                if (approachingVector != null)
                {
                    var movesLengthRest = (float) _moves.Length - moveOnTick;
                    var min = MathTools.Min(approachingVector.X, move.X);
                    var max = MathTools.Max(approachingVector.Y, move.Y) / movesLengthRest;

                    move = new TwoDVector(min, max);
                }

                move = move.AntiClockwiseTurn(casterAim);
            }

            // GenBullet 生成子弹
            IHitStuff? bullet = null;
            if (NowOnTick == 0 && _lockArea != null)
            {
                bullet = _lockArea.ActiveArea(casterPos, casterAim);
            }

            if (_launchTickToBullet.TryGetValue(NowOnTick, out var nowBullet))
            {
                bullet = nowBullet.ActiveBullet(casterPos, casterAim);
            }

            // 是否退出Snipe状态

            var snipeOff = NowOnTick > 0 && NowOnTick == SnipeBreakTick;


            //GONext

            NowTough += TempConfig.ToughGrowPerTick;
            NowOnTick += 1;


            return (move, bullet, snipeOff);
        }

        public bool LaunchSkill(int nowSnipeStep)
        {
            if (nowSnipeStep < SnipeStepNeed) return false;
            NowTough = _baseTough;
            NowOnTick = 0;
            return true;
        }

        // public static Skill InitFormConfig(game_config.Skill skillConfig)
        // {
        //     
        // }
    }
}