using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public interface IHitMedia : IPosMedia
    {
        public Zone RdZone { get; }

        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        IRelationMsg? IsHitBody(IIdPointShape targetBody);
        public IEnumerable<IRelationMsg> HitTeam(IQSpace qSpace);
        public ObjType TargetType { get; }
        public bool IsHit(ICanBeHit characterBody);
    }

    public static class HitAbleMediaStandard
    {
        public static IEnumerable<IRelationMsg> HitTeam(IQSpace qSpace, IHitMedia hitMedia)
        {
            var mapToGidList = qSpace.MapToIEnumNotNullSth((body, aHitMedia) => aHitMedia.IsHitBody(body),
                hitMedia, hitMedia.RdZone.MoveToAnchor(hitMedia.Pos));
            return mapToGidList!;
        }
    }
}