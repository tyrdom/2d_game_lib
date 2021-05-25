using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public readonly struct CharGoTickResult
    {
        public CharGoTickResult(bool stillActive = true, ITwoDTwoP? move = null,
            IPosMedia? launchBullet = null,
            HashSet<IMapInteractable>? dropThing = null,
            IMapInteractable? getThing = null, ValueTuple<MapInteract, CharacterBody>? mapInteractiveAbout = null,
            TelePortMsg? teleTo = null)
        {
            MapInteractive = mapInteractiveAbout?.Item1;
            StillActive = stillActive;
            Move = move;
            LaunchBullet = launchBullet;
            DropThing = dropThing ?? new HashSet<IMapInteractable>();
            GetThing = getThing;
            WhoInteractCall = mapInteractiveAbout?.Item2;
            TeleportTo = teleTo;
        }


        public bool StillActive { get; }
        public ITwoDTwoP? Move { get; }
        public IPosMedia? LaunchBullet { get; }
        public HashSet<IMapInteractable> DropThing { get; }
        public IMapInteractable? GetThing { get; }
        public MapInteract? MapInteractive { get; }
        public CharacterBody? WhoInteractCall { get; }

        public TelePortMsg? TeleportTo { get; }
    }
}