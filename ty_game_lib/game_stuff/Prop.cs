using System;
using System.Collections.Generic;
using System.Diagnostics;
using collision_and_rigid;

namespace game_stuff
{
    public class Prop : ICharAct, ISaleStuff, ICanDrop
    {
        public Prop(int recyclePropStack, int stackCost, uint totalTick, float moveMulti, Bullet propBullet,
            uint launchTick, float moveMustMulti, float minCos)
        {
            RecyclePropStack = recyclePropStack;
            StackCost = stackCost;
            TotalTick = totalTick;
            MoveMulti = moveMulti;
            PropBullet = propBullet;
            LaunchTick = launchTick;
            MoveMustMulti = moveMustMulti;
            MinCos = minCos;
            LastMoveVector = null;
        }


        public int StackCost { get; }
        public uint TotalTick { get; }
        private float MoveMulti { get; }
        private Bullet PropBullet { get; }
        private uint LaunchTick { get; }
        public int RecyclePropStack { get; }

        //推进器类专用
        private float? MoveMustMulti { get; }

        private TwoDVector? LastMoveVector { get; set; }

        private float MinCos { get; }

        public (ITwoDTwoP? move, IEffectMedia? bullet, bool snipeOff, ICanPutInMapInteractable? getFromCage, MapInteract
            interactive) GoATick(TwoDPoint getPos,
                TwoDVector sightAim,
                TwoDVector? rawMoveVector, TwoDVector? limitV)
        {
            var b = NowOnTick == 0;

            var bullet = NowOnTick == LaunchTick ? PropBullet.ActiveBullet(getPos, sightAim) : null;
            var twoDVector = rawMoveVector?.Multi(MoveMulti);
            if (MoveMustMulti != null && limitV != null) // 推进类prop
            {
                var moveVector = rawMoveVector ?? limitV;
                var dVector = LastMoveVector?.GetUnit2();
                var nVector = moveVector.GetUnit2();
                if (dVector != null && nVector != null && LastMoveVector != null)
                {
                    var cos = dVector.Dot(nVector);
                    if (cos < MinCos)
                    {
                        var sin = dVector.Cross(nVector);
                        var clockwiseTurn = LastMoveVector.ClockwiseTurn(new TwoDVector(cos, sin));
                        twoDVector = clockwiseTurn;
                    }
                    else
                    {
                        twoDVector = moveVector.Multi(MoveMustMulti.Value);
                    }
                }

                LastMoveVector = twoDVector;
            }

            NowOnTick++;
            return (twoDVector, bullet, b, null, MapInteract.PickCall);
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

        public IMapInteractable DropAsIMapInteractable(TwoDPoint pos)
        {
            return CanPutInMapInteractableStandard.GenIMapInteractable(pos, this);
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
                case MapInteract.PickCall:
                    var pickAProp = characterStatus.PickAProp(this);
                    return pickAProp == null ? new List<IMapInteractable>() : new List<IMapInteractable> {pickAProp};
                default:
                    throw new ArgumentOutOfRangeException(nameof(interactive), interactive, null);
            }
        }
    }
}