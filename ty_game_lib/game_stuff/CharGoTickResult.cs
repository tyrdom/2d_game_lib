using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class CharGoTickResult : IGameUnitTickResult
    {
        public CharGoTickResult(bool stillAlive = true, ITwoDTwoP? move = null, IPosMedia? launchBullet = null,
            List<IMapInteractable>? dropThing = null,
            IMapInteractable? getThing = null, ValueTuple<MapInteract, CharacterBody>? mapInteractiveAbout = null
        )
        {
            MapInteractive = mapInteractiveAbout?.Item1;
            StillAlive = stillAlive;
            Move = move;
            LaunchBullet = launchBullet;
            DropThing = dropThing ?? new List<IMapInteractable>();
            GetThing = getThing;
            WhoInteractCall = mapInteractiveAbout?.Item2;
        }


        public bool StillAlive { get; }
        public ITwoDTwoP? Move { get; }
        public IPosMedia? LaunchBullet { get; }
        public List<IMapInteractable> DropThing { get; }
        public IMapInteractable? GetThing { get; }
        public MapInteract? MapInteractive { get; }
        public CharacterBody? WhoInteractCall { get; }
    }

    public interface IGameUnitTickResult
    {
    }
}