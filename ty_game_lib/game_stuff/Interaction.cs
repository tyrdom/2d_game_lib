using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Interaction : ICharAct
    {
        public Interaction(int nowTough, uint totalTick, ICanPutInCage inCage, TwoDPoint? interactionPos,
            MapInteract mapInteract)
        {
            NowTough = nowTough;
            TotalTick = totalTick;
            InCage = inCage;
            InteractionPos = interactionPos;
            Interact = mapInteract;
            NowOnTick = 0;
        }

        public (ITwoDTwoP? move, IHitStuff? bullet, bool snipeOff, ICanPutInCage? getFromCage, MapInteract
            interactive) GoATick(TwoDPoint getPos,
                TwoDVector sightAim,
                TwoDVector? rawMoveVector, TwoDVector? limitV)
        {
            var b = NowOnTick == 0;
            var twd = b ? InteractionPos : null;
            var inCage = NowOnTick == TotalTick - 1 ? InCage : null;
            NowOnTick++;
            return (twd, null, b, inCage, Interact);
        }

        public MapInteract Interact { get;  }

        public int NowTough { get; set; }
        public uint NowOnTick { get; set; }
        public uint TotalTick { get; }
        public ICanPutInCage InCage { get; }
        public TwoDPoint? InteractionPos { get; }

        public SkillPeriod InWhichPeriod()
        {
            return NowOnTick < TotalTick ? SkillPeriod.Casting : SkillPeriod.End;
        }

        public int? ComboInputRes()
        {
            return null;
        }

        public void Launch()
        {
            NowOnTick = 0;
        }
    }
}