using collision_and_rigid;

namespace game_stuff
{
    public class Interaction : ICharAct
    {
        public Interaction(int nowTough, uint totalTick, ICanPutInCage inCage)
        {
            NowTough = nowTough;
            TotalTick = totalTick;
            InCage = inCage;
            Interactive = MapInteractive.PickOrInVehicle;
            NowOnTick = 0;
        }

        public (TwoDVector? move, IHitStuff? bullet, bool snipeOff, ICanPutInCage? getFromCage, MapInteractive) GoATick(
            TwoDPoint getPos,
            TwoDVector sightAim,
            TwoDVector? rawMoveVector, TwoDVector? limitV)
        {
            var b = NowOnTick == 0;
            var inCage = NowOnTick == TotalTick - 1 ? InCage : null;
            return (null, null, b, inCage,Interactive);
        }

        public MapInteractive Interactive { get; set; }

        public int NowTough { get; set; }
        public uint NowOnTick { get; set; }
        public uint TotalTick { get; }
        public ICanPutInCage InCage { get; }

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