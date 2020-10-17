using System;
using System.Collections.Generic;
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

        public (ITwoDTwoP? move, IHitStuff? bullet, bool snipeOff, ICanPutInCage? getFromCage, MapInteract
            interactive) GoATick(TwoDPoint getPos,
                TwoDVector sightAim,
                TwoDVector? rawMoveVector, TwoDVector? limitV)
        {
            var b = NowOnTick == 0;

            var bullet = NowOnTick == TotalTick - 1 ? PropBullet.ActiveBullet(getPos, sightAim) : null;
            var twoDVector = rawMoveVector?.Multi(MoveMulti);
            NowOnTick++;
            return (twoDVector, bullet, b, null, MapInteract.PickPropOrWeaponCall);
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
            if (InWhichMapInteractive == null)
            {
                return new CageCanPick(this, pos);
            }

            InWhichMapInteractive.ReLocate(pos);
            return InWhichMapInteractive;
        }

        public bool CanInterActOneBy(CharacterStatus characterStatus)
        {
            return true;
        }

        public bool CanInterActTwoBy(CharacterStatus characterStatus)
        {
            return true;
        }

        public IEnumerable<IMapInteractable> ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            switch (interactive)
            {
                case MapInteract.RecycleCall:
                    characterStatus.RecycleAProp(this);
                    return new List<IMapInteractable>();
                case MapInteract.PickPropOrWeaponCall:
                    var pickAProp = characterStatus.PickAProp(this);
                    return pickAProp == null ? new List<IMapInteractable>() : new List<IMapInteractable> {pickAProp};
                default:
                    throw new ArgumentOutOfRangeException(nameof(interactive), interactive, null);
            }
        }
    }
}