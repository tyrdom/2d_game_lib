using collision_and_rigid;

namespace game_stuff
{
    public class Prop : ICharAct
    {
        private int StackCost;

        public uint TotalTick { get; }


     

        public (TwoDVector? move, IHitStuff? bullet, bool snipeOff) GoATick(TwoDPoint getPos, TwoDVector sightAim,
            TwoDVector? limitV)
        {
            throw new System.NotImplementedException();
        }

        public int NowTough { get; set; }
        public uint NowOnTick { get; set; }

        public SkillPeriod? InWhichPeriod()
        {
            throw new System.NotImplementedException();
        }

        public int? ComboInputRes()
        {
            throw new System.NotImplementedException();
        }
    }
}