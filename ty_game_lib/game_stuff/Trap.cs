using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using collision_and_rigid;

namespace game_stuff
{
    public class Trap : ICanBeHit, IBattleUnitStatus
    {
        public Trap(CharacterStatus characterStatus, SurvivalStatus? survivalStatus, bool canBeSee, TwoDPoint pos,
            int tid,
            BodySize bodySize, uint callTrapTick, uint? maxLifeTimeTick, uint nowLifeTimeTick, IHitMedia trapMedia,
            uint trickDelayTick, uint nowTrickDelayTick, int? trickStack, IHitMedia? launchMedia, int? failChance)
        {
            Owner = characterStatus;
            SurvivalStatus = survivalStatus;
            CanBeSee = canBeSee;
            Pos = pos;
            Tid = tid;
            BodySize = bodySize;
            CallTrapTick = callTrapTick;
            MaxLifeTimeTick = maxLifeTimeTick;
            NowLifeTimeTick = nowLifeTimeTick;
            TrapMedia = trapMedia;
            TrickDelayTick = trickDelayTick;
            NowTrickDelayTick = nowTrickDelayTick;
            OnTrick = false;
            TrickStack = trickStack;
            LaunchMedia = launchMedia;

            FailChance = failChance;
            IdPointBox = null;
        }


        private CharacterStatus Owner { get; }


        private SurvivalStatus? SurvivalStatus { get; }

        private bool CanBeSee { get; }
        private TwoDPoint Pos { get; }
        private int Tid { get; }
        private int? FailChance { get; set; }
        private BodySize BodySize { get; }

        public uint CallTrapTick { get; }

        public uint? MaxLifeTimeTick { get; }

        public uint NowLifeTimeTick { get; set; }
        public IHitMedia TrapMedia { get; }

        public uint TrickDelayTick { get; }

        public uint NowTrickDelayTick { get; set; }
        public bool OnTrick { get; set; }

        public int? TrickStack { get; set; }

        public IHitMedia? LaunchMedia { get; }

        public int GetTeam()
        {
            return Owner.CharacterBody.Team;
        }

        public void StartTrick()
        {
            if (LaunchMedia == null) return;
            OnTrick = true;
            NowTrickDelayTick = 0;
        }

        public TrapGoTickResult GoATick()
        {
            var goATickAndCheckAlive =
                (FailChance == null || FailChance > 0) &&
                (TrickStack == null || TrickStack > 0)
                && (SurvivalStatus == null || SurvivalStatus.GoATickAndCheckAlive())
                && (MaxLifeTimeTick == null || NowLifeTimeTick < MaxLifeTimeTick);

            if (!goATickAndCheckAlive) return new TrapGoTickResult(false, null, this);
            IHitMedia? hitMedia = null;

            if (OnTrick)
            {
                if (NowTrickDelayTick >= TrickDelayTick)
                {
                    if (TrickStack != null) TrickStack -= 1;
                    hitMedia = LaunchMedia;
                    OnTrick = false;
                }

                NowTrickDelayTick += 1;
            }
            else
            {
                var b = NowLifeTimeTick % CallTrapTick == 0;

                if (MaxLifeTimeTick == null) NowLifeTimeTick = 0;

                hitMedia = b ? TrapMedia : null;
            }

            NowLifeTimeTick += 1;

            return
                new TrapGoTickResult(true, hitMedia);
        }


        public bool Include(TwoDPoint pos)
        {
            return false;
        }

        public int GetId()
        {
            return Tid;
        }

        public void BaseBulletAtkOk(int __, int _, IBattleUnitStatus ___)
        {
        }

        public TwoDPoint Move(ITwoDTwoP vector)
        {
            return Pos;
        }

        public BodySize GetSize()
        {
            return BodySize;
        }

        public bool CheckCanBeHit()
        {
            return true;
        }

        public IdPointBox CovToIdBox()
        {
            var zone = Zone.Zero();
            var covToAaBbPackBox = new IdPointBox(zone, this);
            IdPointBox = covToAaBbPackBox;
            return covToAaBbPackBox;
        }

        public TwoDPoint GetAnchor()
        {
            return Pos;
        }

        public TwoDVectorLine GetMoveVectorLine()
        {
            return new TwoDVectorLine(TwoDPoint.Zero(), TwoDPoint.Zero());
        }

        public ITwoDTwoP RelocateWithBlock(WalkBlock walkBlock)
        {
            return Pos;
        }

        public IdPointBox? IdPointBox { get; set; }


        public List<TwoDPoint> GetMayBeSomeThing()
        {
            return Owner.MayBeSomeThing;
        }

        public CharacterStatus? CatchingWho { get; set; }

        public TwoDPoint GetPos()
        {
            return Pos;
        }

        public Damage GenDamage(float damageMulti, bool b)
        {
            return Owner.AttackStatus.GenDamage(damageMulti, b);
        }

        public void LoadCatchTrickSkill(TwoDVector? aim, CatchStunBuffConfig catchAntiActBuffConfig)
        {
        }

        public float GetRr()
        {
            return TempConfig.SizeToR.TryGetValue(BodySize, out var valueOrDefault) ? valueOrDefault : 1f;
        }

        public void FailAtk()
        {
            if (FailChance != null) FailChance -= 1;
        }

        public void TakeDamage(Damage genDamage)
        {
            SurvivalStatus?.TakeDamage(genDamage);
        }
    }
}