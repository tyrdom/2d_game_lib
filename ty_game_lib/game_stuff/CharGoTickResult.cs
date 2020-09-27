using collision_and_rigid;

namespace game_stuff
{
    public class CharGoTickResult
    {
        public CharGoTickResult(ITwoDTwoP? move, IHitStuff? launchBullet, IMapInteractable? dropThing = null,
            IMapInteractable? getThing = null, CharacterBody? whoPickCageCall = null,
            CharacterBody? whoRecycleCageCall = null)
        {
            Move = move;
            LaunchBullet = launchBullet;
            WhoRecycleCageCall = whoRecycleCageCall;
            DropThing = dropThing;
            GetThing = getThing;
            WhoPickCageCall = whoPickCageCall;
        }

        public ITwoDTwoP? Move { get; }
        public IHitStuff? LaunchBullet { get; }
        public IMapInteractable? DropThing { get; }
        public IMapInteractable? GetThing { get; }
        public CharacterBody? WhoPickCageCall { get; }
        public CharacterBody? WhoRecycleCageCall { get; }
    }
}