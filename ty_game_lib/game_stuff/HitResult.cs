using collision_and_rigid;

namespace game_stuff
{
    public class HitResult
    {
        public HitResult(ICanBeHit hitBody, bool killBullet, CharacterBody casterOrOwner, IHitMedia hitMedia)
        {
            HitBody = hitBody;
            KillBullet = killBullet;
            CasterOrOwner = casterOrOwner;
            HitMedia = hitMedia;
        }

        public bool KillBullet { get; }

        public CharacterBody CasterOrOwner { get; }
        public ICanBeHit HitBody { get; }

        public IHitMedia HitMedia { get; }
    }
}