using System;
using collision_and_rigid;

namespace game_stuff
{
    public class Prop : ICharAct, ICanPutInCage
    {
        public int StackCost { get; }

        public uint TotalTick { get; }

        public float MoveMulti { get; }


        public Bullet PropBullet { get; }

        public (TwoDVector? move, IHitStuff? bullet, bool snipeOff, ICanPutInCage? getFromCage) GoATick(TwoDPoint getPos,
            TwoDVector sightAim,
            TwoDVector? rawMoveVector, TwoDVector? limitV = null)
        {
            var b = NowOnTick == 0;

            var bullet = NowOnTick == (TotalTick - 1) ? PropBullet : null;
            var twoDVector = rawMoveVector?.Multi(MoveMulti);
            NowOnTick += 1;
            return (twoDVector, bullet, b, null);
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

        public bool Launch(int nowStack)
        {
            if (nowStack < StackCost) return false;
            NowOnTick = 0;
            return true;
        }

        public IMapInteractable? InWhichMapInteractive { get; set; }
    }
}