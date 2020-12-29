using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public readonly struct CharGoTickResult : IGameUnitTickResult
    {
        public CharGoTickResult(bool stillActive = true, ITwoDTwoP? move = null,
            IPosMedia? launchBullet = null,
            List<IMapInteractable>? dropThing = null,
            IMapInteractable? getThing = null, ValueTuple<MapInteract, CharacterBody>? mapInteractiveAbout = null,
            int? teleTo = null)
        {
            MapInteractive = mapInteractiveAbout?.Item1;
            StillActive = stillActive;
            Move = move;
            LaunchBullet = launchBullet;
            DropThing = dropThing ?? new List<IMapInteractable>();
            GetThing = getThing;
            WhoInteractCall = mapInteractiveAbout?.Item2;
            TeleportToMapId = teleTo;
        }


        public bool StillActive { get; }
        public ITwoDTwoP? Move { get; }
        public IPosMedia? LaunchBullet { get; }
        public List<IMapInteractable> DropThing { get; }
        public IMapInteractable? GetThing { get; }
        public MapInteract? MapInteractive { get; }
        public CharacterBody? WhoInteractCall { get; }

        public int? TeleportToMapId { get; }
    }

    public interface IGameUnitTickResult
    {
    }
}