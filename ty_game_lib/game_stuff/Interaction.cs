using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Interaction : ICharAct
    {
        public Interaction(int nowTough, uint totalTick, ICanPutInMapInteractable inMapInteractable, TwoDPoint? interactionPos,
            MapInteract mapInteract)
        {
            NowTough = nowTough;
            TotalTick = totalTick;
            InMapInteractable = inMapInteractable;
            InteractionPos = interactionPos;
            Interact = mapInteract;
            NowOnTick = 0;
        }

        public (ITwoDTwoP? move, IEffectMedia? bullet, bool snipeOff, ICanPutInMapInteractable? getFromCage, MapInteract
            interactive) GoATick(TwoDPoint getPos,
                TwoDVector sightAim,
                TwoDVector? rawMoveVector, TwoDVector? limitV)
        {
            var b = NowOnTick == 0;
            var twd = b ? InteractionPos : null;
            var inCage = NowOnTick == TotalTick - 1 ? InMapInteractable : null;
            NowOnTick++;
            return (twd, null, b, inCage, Interact);
        }

        public MapInteract Interact { get;  }

        public int NowTough { get; set; }
        public uint NowOnTick { get; set; }
        public uint TotalTick { get; }
        public ICanPutInMapInteractable InMapInteractable { get; }
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