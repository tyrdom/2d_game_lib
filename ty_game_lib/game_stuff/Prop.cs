using System;
using collision_and_rigid;

namespace game_stuff
{
    public class Prop : ICharAct
    {
        private int StackCost;

        public uint TotalTick { get; }

        public float MoveMulti { get; }

        public (TwoDVector? move, IHitStuff? bullet, bool snipeOff) GoATick(TwoDPoint getPos, TwoDVector sightAim,
            TwoDVector? rawMoveVector)
        {
            throw new System.NotImplementedException();
        }

        public int NowTough { get; set; }
        public uint NowOnTick { get; set; }

        public SkillPeriod InWhichPeriod()
        {
            return NowOnTick < TotalTick ? SkillPeriod.Casting : SkillPeriod.End;
        }

        public int? ComboInputRes()
        {
            return null;
        }
    }
}