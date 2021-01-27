using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface IHitMedia : IPosMedia
    {
        public Zone RdZone { get; }

        public Dictionary<size, BulletBox> SizeToBulletCollision { get; }
        IRelationMsg? IsHitBody(IIdPointShape targetBody, SightMap blockMap);
        public IEnumerable<IRelationMsg> HitTeam(IQSpace qSpace, SightMap blockMap);
        public ObjType TargetType { get; }
    }

    public static class HitAbleMediaStandard
    {
        public static IEnumerable<IRelationMsg> HitTeam(IQSpace qSpace, IHitMedia hitMedia, SightMap blockMap)
        {
            var mapToGidList = qSpace.MapToIEnumNotNullSth((body, aHitMedia) => aHitMedia.IsHitBody(body, blockMap),
                hitMedia, hitMedia.RdZone.MoveToAnchor(hitMedia.Pos));
            return mapToGidList!;
        }
    }
}