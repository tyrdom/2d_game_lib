using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class TeleportUnit : IApplyUnit
    {
        public IMapInteractable? InWhichMapInteractive { get; set; }

        public PlayGround FromPlayGround { get; set; }
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
            throw new NotImplementedException();
        }
    }
}