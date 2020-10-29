using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class CharGoTickResult
    {
        public CharGoTickResult(ITwoDTwoP? move = null, IEffectMedia? launchBullet = null,
            List<IMapInteractable>? dropThing = null,
            IMapInteractable? getThing = null, ValueTuple<MapInteract, CharacterBody>? mapInteractiveAbout = null
        )
        {
            MapInteractive = mapInteractiveAbout?.Item1;
            Move = move;
            LaunchBullet = launchBullet;
            DropThing = dropThing ?? new List<IMapInteractable>();
            GetThing = getThing;
            WhoInteractCall = mapInteractiveAbout?.Item2;
        }


        public ITwoDTwoP? Move { get; }
        public IEffectMedia? LaunchBullet { get; }
        public List<IMapInteractable> DropThing { get; }
        public IMapInteractable? GetThing { get; }
        public MapInteract? MapInteractive { get; }
        public CharacterBody? WhoInteractCall { get; }
    }
}