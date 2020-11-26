using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public interface IHitMedia : IEffectMedia ,IPosMedia
    {
        public Zone RdZone { get; }

        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        bool IsHitBody(IIdPointShape targetBody);
        public HashSet<int> HitTeam(IQSpace qSpace);
        public ObjType TargetType { get; }
        public bool IsHit(ICanBeHit characterBody);
    }

    public static class HitAbleMediaStandard
    {
        public static HashSet<int> HitTeam(IQSpace qSpace, IHitMedia hitMedia)
        {
            var mapToGidList = qSpace.FilterToGIdPsList((body, aHitMedia) => aHitMedia.IsHitBody(body),
                hitMedia, hitMedia.RdZone.MoveToAnchor(hitMedia.Pos));
            return SomeTools.EnumerableToHashSet(mapToGidList.Select(x => x.GetId()));
        }
    }
}