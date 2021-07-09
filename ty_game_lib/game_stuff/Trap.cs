using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Trap : ICanBeAndNeedHit, IBattleUnitStatus, INotMoveCanBeAndNeedSew
    {
        public Trap(CharacterStatus characterStatus, SurvivalStatus? survivalStatus, bool canBeSee, TwoDPoint pos,
            size bodySize, uint callTrapTick, uint? maxLifeTimeTick, uint nowLifeTimeTick, IHitMedia trapMedia,
            uint trickDelayTick, uint nowTrickDelayTick, int? trickStack, IHitMedia? launchMedia, int? failChanceStack,
            float damageMulti,
            int instanceId, trap_id trapId)
        {
            NotOverFlow = true;
            Owner = characterStatus;
            SurvivalStatus = survivalStatus;
            CanBeSee = canBeSee;
            Pos = pos;
            BodySize = bodySize;
            CallTrapTick = callTrapTick;
            MaxLifeTimeTick = maxLifeTimeTick;
            NowLifeTimeTick = nowLifeTimeTick;
            TrapMedia = trapMedia;
            TrapMedia.Pos = pos;
            TrapMedia.Aim = Owner.GetAim();
            TrickDelayTick = trickDelayTick;
            NowTrickDelayTick = nowTrickDelayTick;
            OnTrick = false;
            TrickStack = trickStack;
            LaunchMedia = launchMedia;
            if (LaunchMedia != null)
            {
                LaunchMedia.Pos = pos;
                LaunchMedia.Aim = Owner.GetAim();
            }

            FailChanceStack = failChanceStack;
            DamageMulti = damageMulti;

            var zone = Zone.Zero();
            var covToAaBbPackBox = new IdPointBox(zone, this);
            IdPointBox = covToAaBbPackBox;
            TrapMedia.Sign(this);
            LaunchMedia?.Sign(this);

            MapMarkId = instanceId;
            TrapId = trapId;
        }

        public override string ToString()
        {
            return $"trapId :{MapMarkId} pos: {Pos}";
        }


        public bool NotOverFlow { get; set; }
        private CharacterStatus Owner { get; }
        private SurvivalStatus? SurvivalStatus { get; }
        public bool CanBeSee { get; }
        private TwoDPoint Pos { get; }

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
                && (SurvivalStatus == null || SurvivalStatus.GoATickAndCheckAlive())
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

                if (MaxLifeTimeTick == null && b) NowLifeTimeTick = 0;

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
            return MapMarkId;
        }


        public void BaseBulletAtkOk(int __, int _, IBattleUnitStatus ___)
        {
        }

        public TwoDPoint Move(ITwoDTwoP vector)
        {
            return Pos;
        }

        // public IdPointBox InBox { get; set; }

        public size GetSize()
        {
            return BodySize;
        }

        public bool CheckCanBeHit()
        {
            return true;
        }


        public IBattleUnitStatus GetBattleUnitStatus()
        {
            return this;
        }

        public ISeeTickMsg GenTickMsg(int? gid = null)
        {
            return new TrapTickMsg(Pos, SurvivalStatus?.GenShortStatus() ?? -1f, Owner.GId, (int) TrapId);
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

        public IdPointBox IdPointBox { get; set; }


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
            return StuffLocalConfig.GetRBySize(BodySize);
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
            if (SurvivalStatus != null)
            {
                var survivalStatus = SurvivalStatus;
                var isDead = survivalStatus.IsDead();
                survivalStatus.TakeDamage(genDamage);
                return new DmgShow(!isDead && survivalStatus.IsDead(), genDamage);
            }
            else
                return (DmgShow?) null;
        }

        public int MapMarkId { get; set; }

        public trap_id TrapId { get; }
    }

    public class TrapTickMsg : ISeeTickMsg
    {
        public TrapTickMsg(TwoDPoint pos, float ssStatus, int ownerId, int trapId)
        {
            Pos = pos;
            SsStatus = ssStatus;
            OwnerId = ownerId;
            TrapId = trapId;
        }

        public int OwnerId { get; }
        public int TrapId { get; }
        public TwoDPoint Pos { get; }
        public float SsStatus { get; }
    }
}