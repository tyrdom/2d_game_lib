using collision_and_rigid;

namespace game_stuff
{
    public interface IRelationMsg
    {
        public CharacterStatus CasterOrOwner { get; }
        public ICanBeHit WhoTake { get; }
    }

    internal interface IDamageMsg : IRelationMsg
    {
        DmgShow DmgShow { get; }
    }

    public readonly struct BulletHit : IDamageMsg
    {
        public BulletHit(ICanBeHit whoTake, DmgShow dmgShow, CharacterStatus casterOrOwner, IHitMedia hitMedia)
        {
            WhoTake = whoTake;
            DmgShow = dmgShow;
            CasterOrOwner = casterOrOwner;
            HitMedia = hitMedia;
        }

        public DmgShow DmgShow { get; }

        public CharacterStatus CasterOrOwner { get; }
        public ICanBeHit WhoTake { get; }

        public IHitMedia HitMedia { get; }
    }
}