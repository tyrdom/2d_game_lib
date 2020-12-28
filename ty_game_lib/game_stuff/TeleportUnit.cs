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
        public PlayGround ToPlayGround { get; set; }

        public TwoDPoint ToPos { get; set; }

        public BodySize[] AllowSizes { get; }

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

        public IEnumerable<IMapInteractable> ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            switch (interactive)
            {
                case MapInteract.GetInfoCall:
                    //todo get mapinfo to character
                    return new IMapInteractable[] { };
                case MapInteract.BuyOrApplyCall:
                    var removeBody = FromPlayGround?.RemoveBody(characterStatus.CharacterBody);
                    if (removeBody == null || !removeBody.Value)
                        throw new IndexOutOfRangeException($"remove body fail {characterStatus.GId}");
                    ToPlayGround.AddBody(characterStatus.CharacterBody, ToPos);
                    return new IMapInteractable[] { };

                default:
                    throw new ArgumentOutOfRangeException(nameof(interactive), interactive, null);
            }
        }
    }
}