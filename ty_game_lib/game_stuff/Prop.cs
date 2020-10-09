using System;
using collision_and_rigid;

namespace game_stuff
{
    public class Prop : ICharAct, ICanPutInCage
    {
        public Prop(int recyclePropStack, int stackCost, uint totalTick, float moveMulti, Bullet propBullet)
        {
            RecyclePropStack = recyclePropStack;
            StackCost = stackCost;
            TotalTick = totalTick;
            MoveMulti = moveMulti;
            PropBullet = propBullet;
        }

        public int StackCost { get; }

        public uint TotalTick { get; }

        public float MoveMulti { get; }

        public Bullet PropBullet { get; }

        public int RecyclePropStack { get; }

        public (ITwoDTwoP? move, IHitStuff? bullet, bool snipeOff, ICanPutInCage? getFromCage, MapInteractive) GoATick(
            TwoDPoint getPos,
            TwoDVector sightAim,
            TwoDVector? rawMoveVector, TwoDVector? limitV)
        {
            var b = NowOnTick == 0;

            var bullet = NowOnTick == (TotalTick - 1) ? PropBullet.ActiveBullet(getPos, sightAim) : null;
            var twoDVector = rawMoveVector?.Multi(MoveMulti);
            NowOnTick++;
            return (twoDVector, bullet, b, null, MapInteractive.PickOrInVehicle);
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

        public IMapInteractable GenIMapInteractable(TwoDPoint pos)
        {
            return GameTools.GenIMapInteractable(pos, InWhichMapInteractive, this);
        }

        public bool CanPick(CharacterStatus characterStatus)
        {
            return true;
        }
    }
}