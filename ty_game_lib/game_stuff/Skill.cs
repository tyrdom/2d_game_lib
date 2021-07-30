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

        public int GetIntId()
        {
            return (int) SkillId;
        }

        private skill_id SkillId { get; }
        public int NowTough { get; set; }

        public int SnipeStepNeed { get; }
        public int AmmoCost { get; }

        private int BaseTough { get; }

        private int CanInputMove { get; }
        private Dictionary<uint, Bullet[]> LaunchTickToBullets { get; }
        private TwoDVector[] Moves { get; }

        public Skill? EnemyFailTrickSkill { get; }
        private uint MoveStartTick { get; }

        private uint SkillMustTick { get; } //必须播放帧，播放完才能进行下一个技能
        private uint ComboInputStartTick { get; } //可接受输入操作帧

        public uint TotalTick { get; }
        // 至多帧，在播放帧

        private int NextCombo { get; }

        private uint SnipeBreakTick { get; }

        private PlayingBuffsMaker GetBuffsWhenAbsorb { get; }

        private PlayingBuffsMaker SetBuffsToOpponentWhenAbsorb { get; }
        private Dictionary<size, IStunBuffMaker>? SetStunBuffsToOpponentWhenAbsorb { get; }


        public string LogUser()
        {
            return LaunchTickToBullets.SelectMany(keyValuePair => keyValuePair.Value).Select(x => x.Caster)
                .Select(characterStatus =>
                    characterStatus?.GetId().ToString() == null ? "!null!" : characterStatus.GetId().ToString())
                .Aggregate("", (current, @null) => current + @null);
        }

        private Skill(Dictionary<uint, Bullet[]> launchTickToBullets, TwoDVector[] moves,
            uint moveStartTick, uint skillMustTick, uint totalTick,
            int baseTough, uint comboInputStartTick, int nextCombo,
            LockArea? lockArea, uint snipeBreakTick, int snipeStepNeed, int ammoCost, int canInputMove,
            skill_id skillId, Skill? enemyFailTrickSkill, PlayingBuffsMaker getBuffsWhenAbsorb,
            Dictionary<size, IStunBuffMaker>? setStunBuffsToOpponentWhenAbsorb,
            PlayingBuffsMaker setBuffsToOpponentWhenAbsorb)
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
            LaunchTickToBullets = launchTickToBullets;
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
            EnemyFailTrickSkill = enemyFailTrickSkill;
            GetBuffsWhenAbsorb = getBuffsWhenAbsorb;
            SetStunBuffsToOpponentWhenAbsorb = setStunBuffsToOpponentWhenAbsorb;
            SetBuffsToOpponentWhenAbsorb = setBuffsToOpponentWhenAbsorb;
        }


        public void PickedBySomeOne(IBattleUnitStatus characterStatus)
        {
            LockArea?.Sign(characterStatus);
            foreach (var bullet1 in LaunchTickToBullets.Values.SelectMany(bullet => bullet))
            {
                bullet1.Sign(characterStatus);
            }

            EnemyFailTrickSkill?.PickedBySomeOne(characterStatus);
        }

        public static Skill GenSkillById(string id)
        {
            var o = (skill_id) Enum.Parse(typeof(skill_id), id, true);
            return GenSkillById(o);
        }

        private static Skill GenSkillById(skill_id id)
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
                    var genByConfig = pairValue.Select(x => Bullet.GenById(x, pair.Key)).ToArray();
                    return genByConfig;
                });

            var byConfig = LockArea.TryGenById(skill.LockArea, out var area)
                ? area
                : null;
            var skillBaseTough = skill.BaseTough == 0
                ? dictionary.Any() ? (int) dictionary.Keys.Min() : 0
                : skill.BaseTough;

            var genSkillById = skill.EnemyFailTrickSkill == "" ? null : GenSkillById(skill.EnemyFailTrickSkill);

            var immutableDictionary = new PlayingBuffsMaker(skill.GetPlayingBuffWhenAbsorb);
            var playingBuffs = new PlayingBuffsMaker(skill.SetPlayingBuffToOpponentWhenAbsorb);

            var buffMakers = skill.SetStunBuffToOpponentWhenAbsorb.Any()
                ? Bullet.GAntiActBuffConfigs(skill.SetStunBuffToOpponentWhenAbsorb)
                : null;

            return new Skill(dictionary, twoDVectors, skill.MoveStartTime,
                skill.SkillMustTime, skill.SkillMaxTime,
                skillBaseTough, skill.ComboInputStartTime, skill.NextCombo, byConfig,
                skill.BreakSnipeTime,
                skill.SnipeStepNeed - 1, skill.AmmoCost, (int) skill.CanInputMove, skill.id, genSkillById,
                immutableDictionary, buffMakers, playingBuffs);
        }


        public SkillPeriod InWhichPeriod()
        {
            if (NowOnTick < SkillMustTick)
            {
                return SkillPeriod.Casting;
            }

            return NowOnTick < TotalTick ? SkillPeriod.CanCombo : SkillPeriod.End;
        }

        public int? ComboInputRes() //可以输入连击，返回 下一个动作
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

        public (ITwoDTwoP? move, IEnumerable<IEffectMedia> bullet, bool snipeOff, ICanPutInMapInteractable? getFromCage,
            MapInteract interactive) GoATick(TwoDPoint casterPos,
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
            HashSet<IEffectMedia> bullet = new HashSet<IEffectMedia>();

            if (NowOnTick == 0 && LockArea != null)
            {
                bullet.Add(LockArea.Active(casterPos, casterAim));
            }

            NowTough += CommonConfig.OtherConfig.tough_grow;

            if (LaunchTickToBullets.TryGetValue(NowOnTick, out var nowBullet))
            {
#if DEBUG
                Console.Out.WriteLine(
                    $"to launch {nowBullet.Length} bullet ~~~ {NowOnTick} ");
#endif

                var posMediaS = nowBullet.Select(x => x.Active(casterPos, casterAim));

                bullet.UnionWith(posMediaS);
            }

            // 是否退出Snipe状态
            var snipeOff = NowOnTick > 0 && NowOnTick == SnipeBreakTick;

            //GONext

            return (move, bullet, snipeOff, null, MapInteract.PickCall);
        }

        public (IPosMedia[]? posMedia, bool canInputMove) GetSkillStart(TwoDPoint casterPos, TwoDVector casterAim)
        {
            var posMedia = LockArea?.Active(casterPos, casterAim);
            var posMediaS = posMedia == null ? null : new[] {posMedia};
            return (posMediaS, NowOnTick < CanInputMove);
        }

        public bool Launch(int nowSnipeStep, int nowAmmo, out bool isLowAmmo)
        {
            var b = nowAmmo < AmmoCost;
            isLowAmmo = b;
            if (nowSnipeStep < SnipeStepNeed || b)
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

        public void GenAbsorbBuffs(CharacterStatus bodyCaster, CharacterStatus targetCharacterStatus)
        {
            var playingBuffs = GetBuffsWhenAbsorb.GenBuffs();
            targetCharacterStatus.AddPlayingBuff(playingBuffs);
            var genBuffs = SetBuffsToOpponentWhenAbsorb.GenBuffs();
            bodyCaster.AddPlayingBuff(genBuffs);
            if (SetStunBuffsToOpponentWhenAbsorb == null)
            {
                return;
            }

            var buffMaker =
                SetStunBuffsToOpponentWhenAbsorb[targetCharacterStatus.CharacterBody.GetSize()];
            Bullet.NoMediaHit(bodyCaster, targetCharacterStatus, targetCharacterStatus.StunBuff, buffMaker, 3,
                bodyCaster.GetPos(), bodyCaster.GetAim());
            // buffMaker.GenBuff(bodyCaster.GetPos(), targetCharacterStatus.GetPos(), bodyCaster.GetAim(),)
        }
    }
}