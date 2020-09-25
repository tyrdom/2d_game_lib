using collision_and_rigid;

namespace game_stuff
{
    public class CageActiveAct : ICharAct
    {
        public CageActiveAct(int nowTough, uint totalTick, ICanPutInCage inCage, PickCage inWhichCage)
        {
            NowTough = nowTough;
            TotalTick = totalTick;
            InCage = inCage;
            InWhichCage = inWhichCage;
            NowOnTick = 0;
        }

        public (TwoDVector? move, IHitStuff? bullet, bool snipeOff, ICanPutInCage? inCage) GoATick(TwoDPoint getPos,
            TwoDVector sightAim,
            TwoDVector? rawMoveVector)
        {
            var b = NowOnTick == 0;
            var inCage = NowOnTick == TotalTick - 1 ? this.InCage : null;
            return (null, null, b, inCage);
        }

        public PickCage InWhichCage { get; }
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
    }
}