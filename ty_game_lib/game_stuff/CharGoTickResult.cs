using System;
using System.Collections.Immutable;
using collision_and_rigid;

namespace game_stuff
{
    public readonly struct CharGoTickResult
    {
        public CharGoTickResult(bool stillActive = true, ITwoDTwoP? move = null,
            IPosMedia[]? launchBullet = null,
            IMapInteractable? getThing = null, ValueTuple<MapInteract, CharacterBody>? mapInteractiveAbout = null,
            ImmutableArray<IActResult>? actResults = null)
        {
            MapInteractive = mapInteractiveAbout?.Item1;
            StillActive = stillActive;
            Move = move;
            LaunchBullet = launchBullet ?? new IPosMedia[] { };
            GetThing = getThing;
            WhoInteractCall = mapInteractiveAbout?.Item2;
            ActResults = actResults ?? ImmutableArray<IActResult>.Empty;
        }

        public static CharGoTickResult CharGoTickResultEmpty { get; } = new CharGoTickResult();
        public bool StillActive { get; }
        public ITwoDTwoP? Move { get; }
        public IPosMedia[] LaunchBullet { get; }
        public IMapInteractable? GetThing { get; }
        public MapInteract? MapInteractive { get; }
        public CharacterBody? WhoInteractCall { get; }

        public ImmutableArray<IActResult> ActResults { get; }
    }
}