using System.Dynamic;
using System.Linq.Expressions;
using collision_and_rigid;

namespace game_stuff
{
    public class Trap : IIdPointShape, ICanBeHit
    {
        private CharacterStatus Caster { get; }

        private SurvivalStatus SurvivalStatus { get; }

        private bool CanBeSee { get; }
        private TwoDPoint Pos { get; }
        private int tid { get; }

        private BodySize BodySize { get; }

        public uint CallTrapTick { get; }

        public uint MaxLifeTimeTick { get; }

        public uint NowLifeTimeTick { get; }
        public IHitMedia TrapMedia { get; }


        public IHitMedia? LaunchMedia { get; }


        public TrapGoTickResult GoATick()
        {
            var goATickAndCheckAlive = SurvivalStatus.GoATickAndCheckAlive() && NowLifeTimeTick < MaxLifeTimeTick;

            var b = NowLifeTimeTick % CallTrapTick == 0;

            var t = b ? TrapMedia : null;

            return goATickAndCheckAlive
                ? new TrapGoTickResult(true, t)
                : new TrapGoTickResult(false, null, InWhichBox);
        }


        //todo trap
        public bool Include(TwoDPoint pos)
        {
            return false;
        }

        public int GetId()
        {
            return tid;
        }

        public TwoDPoint Move(ITwoDTwoP vector)
        {
            return Pos;
        }

        public BodySize GetSize()
        {
            return BodySize;
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

        public IdPointBox? InWhichBox { get; set; }
    }

    public interface ICanBeHit
    {
        BodySize GetSize();
        TwoDPoint GetAnchor();
    }
}