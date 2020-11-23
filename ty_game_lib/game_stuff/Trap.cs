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

        public uint CallTrapTick { get; }

        public uint MaxLifeTime { get; }

        public uint NowLifeTime { get; }
        public IHitMedia TrapMedia { get; }

        public IHitMedia LaunchMedia { get; }

        public IHitMedia GoATick { get; }

        public BodySize BodySize { get; }

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
            throw new System.NotImplementedException();
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
    }

    public interface ICanBeHit
    {
        BodySize GetSize();
        TwoDPoint GetAnchor();
    }
}