using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class TeleportUnit : IApplyUnit
    {
        public TeleportUnit(IMapInteractable? inWhichMapInteractive,
            PlayGround toPlayGround,
            TwoDPoint toPos, BodySize[] allowSizes)
        {
            InWhichMapInteractive = inWhichMapInteractive;
            FromPlayGround = null;
            ToPlayGround = toPlayGround;
            ToPos = toPos;
            AllowSizes = allowSizes;
        }

        public void PutInPlayGround(PlayGround fromPlayGround)
        {
            FromPlayGround = fromPlayGround;
        }

        public IMapInteractable? InWhichMapInteractive { get; set; }

        public PlayGround? FromPlayGround { get; set; }
        public PlayGround ToPlayGround { get; }


        public TwoDPoint ToPos { get; }

        public BodySize[] AllowSizes { get; }

        public TwoDPoint? GetThisPos()
        {
            return InWhichMapInteractive?.GetAnchor();
        }

        public bool CanInterActOneBy(CharacterStatus characterStatus)
        {
            return true;
        }

        public bool CanInterActTwoBy(CharacterStatus characterStatus)
        {
            var bodySize = characterStatus.CharacterBody.GetSize();
            var contains = AllowSizes.Contains(bodySize);
            return contains;
        }

        public IActResult? ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            switch (interactive)
            {
                case MapInteract.GetInfoCall:
                    return null;
                case MapInteract.BuyOrApplyCall:

                    return new TelePortMsg(ToPlayGround.MgId, ToPos);

                default:
                    throw new ArgumentOutOfRangeException(nameof(interactive), interactive, null);
            }
        }
    }
}