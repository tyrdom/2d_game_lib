using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Interaction : ICharAct
    {
        public Interaction(int nowTough, uint totalTick, ICanPutInMapInteractable inMapInteractable,
            TwoDPoint? interactionPos,
            MapInteract mapInteract, interactionAct id)
        {
            NowTough = nowTough;
            TotalTick = totalTick;
            InMapInteractable = inMapInteractable;
            InteractionPos = interactionPos;
            Interact = mapInteract;
            Id = id;
            NowOnTick = 0;
        }

        public (ITwoDTwoP? move, IEnumerable<IEffectMedia> bullet, bool snipeOff, ICanPutInMapInteractable? getFromCage,
            MapInteract interactive) GoATick(TwoDPoint getPos,
                TwoDVector sightAim,
                TwoDVector? rawMoveVector, TwoDVector? limitV)
        {
            var b = NowOnTick == 0;
            var twd = b ? InteractionPos : null;
            NowOnTick++;
            var inCage = NowOnTick == TotalTick - 1 ? InMapInteractable : null;

            return (twd, new IEffectMedia[] { }, b, inCage, Interact);
        }

        public int GetIntId()
        {
            return (int) Id;
        }

        private MapInteract Interact { get; }

        public interactionAct Id { get; }
        public int NowTough { get; set; }
        public uint NowOnTick { get; set; }
        public uint TotalTick { get; }
        public ICanPutInMapInteractable InMapInteractable { get; }
        private TwoDPoint? InteractionPos { get; }

        public SkillPeriod InWhichPeriod()
        {
            return NowOnTick < TotalTick ? SkillPeriod.Casting : SkillPeriod.End;
        }

        public int? ComboInputRes()
        {
            return null;
        }

        public action_type GetTypeEnum()
        {
            return action_type.interaction;
        }

        public void Launch()
        {
            NowOnTick = 0;
        }
    }
}