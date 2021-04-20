using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface IHitMedia : IPosMedia
    {
        public Zone RdZone { get; }

        public bool IsHit(ICanBeHit characterBody, SightMap? blockMap);
        public Dictionary<size, BulletBox> SizeToBulletCollision { get; }
        public IEnumerable<IRelationMsg> HitTeam(IEnumerable<IQSpace> qSpaces, SightMap? blockMap);
        public ObjType TargetType { get; }
        public bool HitNumLimit(out int num);
        IRelationMsg? HitSth(ICanBeHit canBeHit);
    }

    public static class HitAbleMediaStandard
    {
        public static IEnumerable<IRelationMsg> HitTeam(IEnumerable<IQSpace> qSpace, IHitMedia hitMedia,
            SightMap? blockMap)
        {
            var idPointShapes = qSpace.SelectMany(x => x.FilterToGIdPsList((bd, aHitMedia) =>
                    bd is ICanBeHit canBeHit && aHitMedia.IsHit(canBeHit, blockMap), hitMedia,
                hitMedia.RdZone.MoveToAnchor(hitMedia.Pos)));
            if (!hitMedia.HitNumLimit(out var num))
                return idPointShapes.OfType<ICanBeHit>().Select(hitMedia.HitSth).Where(x => x != null)!;

            var shapes = idPointShapes.ToList();
            shapes.Sort((shape, pointShape) => shape.GetAnchor().GetDistance(hitMedia.Pos)
                .CompareTo(pointShape.GetAnchor().GetDistance(hitMedia.Pos)));
            var pointShapes = shapes.Take(num).OfType<ICanBeHit>();
            var relationMsgS =
                pointShapes.Select(hitMedia.HitSth).Where(x => x != null);
            return relationMsgS!;
        }
    }
}