using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Trap : ICanBeHit, IBattleUnitStatus
    {
        public Trap(CharacterStatus characterStatus, SurvivalStatus? survivalStatus, bool canBeSee, TwoDPoint pos,
            int tid,
            size bodySize, uint callTrapTick, uint? maxLifeTimeTick, uint nowLifeTimeTick, IHitMedia trapMedia,
            uint trickDelayTick, uint nowTrickDelayTick, int? trickStack, IHitMedia? launchMedia, int? failChanceStack,
            float damageMulti)
        {
            NotOverFlow = true;
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
            FailChanceStack = failChanceStack;
            DamageMulti = damageMulti;
            IdPointBox = null;
            TrapMedia.Sign(this);
            LaunchMedia?.Sign(this);
            InBox = CovToIdBox();
        }

        public bool NotOverFlow { get; set; }
        private CharacterStatus Owner { get; }
        private SurvivalStatus? SurvivalStatus { get; }
        public bool CanBeSee { get; }
        private TwoDPoint Pos { get; }
        private int Tid { get; }
        private int? FailChanceStack { get; set; }
        private size BodySize { get; }
        private uint CallTrapTick { get; }
        private uint? MaxLifeTimeTick { get; }
        private uint NowLifeTimeTick { get; set; }
        private IHitMedia TrapMedia { get; }
        private uint TrickDelayTick { get; }

        private uint NowTrickDelayTick { get; set; }
        private bool OnTrick { get; set; }

        private int? TrickStack { get; set; }

        private IHitMedia? LaunchMedia { get; }
        private float DamageMulti { get; }

        public int GetTeam()
        {
            return Owner.CharacterBody.Team;
        }

        public void StartTrick()
        {
            if (LaunchMedia == null && OnTrick) return;
            OnTrick = true;
            NowTrickDelayTick = 0;
        }

        public TrapGoTickResult GoATick()
        {
            var goATickAndCheckAlive =
                NotOverFlow &&
                (FailChanceStack == null || FailChanceStack > 0) &&
                (TrickStack == null || TrickStack > 0)
                && (SurvivalStatus == null || SurvivalStatus.Value.GoATickAndCheckAlive())
                && (MaxLifeTimeTick == null || NowLifeTimeTick < MaxLifeTimeTick);

            if (!goATickAndCheckAlive) return new TrapGoTickResult(false, null, this);
            IHitMedia? hitMedia = null;

            if (OnTrick)
            {
                if (NowTrickDelayTick >= TrickDelayTick)
                {
                    NowTrickDelayTick = 0;
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

        public IdPointBox InBox { get; set; }

        public size GetSize()
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

        public ISeeTickMsg GenTickMsg(int? gid = null)
        {
            return new TrapTickMsg(Pos, Tid, SurvivalStatus?.GenShortStatus() ?? -1f);
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


        public CharacterBody GetFinalCaster()
        {
            return Owner.CharacterBody;
        }

       

        public CharacterStatus? CatchingWho { get; set; }

        public TwoDPoint GetPos()
        {
            return Pos;
        }

        public Damage GenDamage(float damageMulti, bool b)
        {
            return Owner.AttackStatus.GenDamage(damageMulti * DamageMulti, b);
        }

        public void LoadCatchTrickSkill(TwoDVector? aim, CatchStunBuffMaker catchAntiActBuffMaker)
        {
        }

        public float GetRr()
        {
            return LocalConfig.GetRBySize(BodySize);
        }

        public void AddAKillScore(CharacterBody characterBody)
        {
            Owner.AddAKillScore(characterBody);
        }

        public void FailAtk()
        {
            if (FailChanceStack != null) FailChanceStack -= 1;
        }

        public DmgShow? TakeDamage(Damage genDamage)
        {
            return SurvivalStatus.HasValue
                ? new DmgShow(SurvivalStatus.Value.TakeDamage(genDamage), genDamage)
                : (DmgShow?) null;
        }
    }

    public class TrapTickMsg : ISeeTickMsg
    {
        public TrapTickMsg(TwoDPoint pos, int tid, float ssStatus)
        {
            Pos = pos;
            Tid = tid;
            SsStatus = ssStatus;
        }

        public TwoDPoint Pos { get; }
        public int Tid { get; }
        public float SsStatus { get; }
    }
}