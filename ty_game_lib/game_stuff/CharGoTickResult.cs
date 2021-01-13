using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public readonly struct CharGoTickResult : IGameUnitTickResult
    {
        public CharGoTickResult(bool stillActive = true, ITwoDTwoP? move = null,
            IPosMedia? launchBullet = null,
            HashSet<IAaBbBox>? dropThing = null,
            IMapInteractable? getThing = null, ValueTuple<MapInteract, CharacterBody>? mapInteractiveAbout = null,
            int? teleTo = null)
        {
            MapInteractive = mapInteractiveAbout?.Item1;
            StillActive = stillActive;
            Move = move;
            LaunchBullet = launchBullet;
            DropThing = dropThing ?? new HashSet<IAaBbBox>();
            GetThing = getThing;
            WhoInteractCall = mapInteractiveAbout?.Item2;
            TeleportToMapId = teleTo;
        }


        public bool StillActive { get; }
        public ITwoDTwoP? Move { get; }
        public IPosMedia? LaunchBullet { get; }
        public HashSet<IAaBbBox> DropThing { get; }
        public IMapInteractable? GetThing { get; }
        public MapInteract? MapInteractive { get; }
        public CharacterBody? WhoInteractCall { get; }

        public int? TeleportToMapId { get; }
    }

    public interface IGameUnitTickResult
    {
    }
}