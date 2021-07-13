using collision_and_rigid;

namespace game_stuff
{
    public interface IRelationMsg
    {
        public CharacterStatus CasterOrOwner { get; }
        public ICanBeAndNeedHit WhoTake { get; }
    }

    public interface IHitMsg : IRelationMsg
    {
        // public IHitMedia HitMedia { get; }
    }

    public interface IDamageMsg : IRelationMsg
    {
        DmgShow DmgShow { get; }
    }

    public readonly struct BulletHit : IDamageMsg, IHitMsg
    {
        public BulletHit(ICanBeAndNeedHit whoTake, DmgShow dmgShow, CharacterStatus casterOrOwner, Bullet hitMedia)
        {
            WhoTake = whoTake;
            DmgShow = dmgShow;
            CasterOrOwner = casterOrOwner;
            HitMedia = hitMedia;
        }


        public DmgShow DmgShow { get; }

        public CharacterStatus CasterOrOwner { get; }
        public ICanBeAndNeedHit WhoTake { get; }
        public Bullet HitMedia { get; }
    }
}