using collision_and_rigid;

namespace game_stuff
{
    public class CharGoTickMsg
    {
        public CharGoTickMsg(ITwoDTwoP? move, IHitStuff? launchBullet, IMapInteractive? dropThing = null,
            IMapInteractive? getThing = null, CharacterBody? whoPickCageCall = null,
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
        public IMapInteractive? DropThing { get; }
        public IMapInteractive? GetThing { get; }
        public CharacterBody? WhoPickCageCall { get; }
        public CharacterBody? WhoRecycleCageCall { get; }
    }
}