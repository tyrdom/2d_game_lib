using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface IHitMedia : IPosMedia
    {
        
        // public 
        public Zone RdZone { get; }

        public bool IsCollisionHit(ICanBeAndNeedHit characterBody, SightMap? blockMap);
        public Dictionary<size, BulletBox> SizeToBulletCollision { get; }
        public IEnumerable<IRelationMsg> HitTeam(IEnumerable<IQSpace> qSpaces, SightMap? blockMap);
        public ObjType TargetType { get; }
        public bool HitNumLimit(out int num);
        IRelationMsg? HitSth(ICanBeAndNeedHit canBeAndNeedHit);
    }

    public static class HitAbleMediaStandard
    {
        public static IEnumerable<IRelationMsg> HitTeam(IEnumerable<IQSpace> qSpace, IHitMedia hitMedia,
            SightMap? blockMap)
        {
            var idPointShapes = qSpace.SelectMany(x => x.FilterToBoxList<IdPointBox, IHitMedia>((bd, aHitMedia) =>
                    bd.IdPointShape is ICanBeAndNeedHit canBeHit && aHitMedia.IsCollisionHit(canBeHit, blockMap), hitMedia,
                hitMedia.RdZone.MoveToAnchor(hitMedia.Pos)));
            if (!hitMedia.HitNumLimit(out var num))
                return idPointShapes.Select(x => x.IdPointShape).OfType<ICanBeAndNeedHit>().Select(hitMedia.HitSth)
                    .Where(x => x != null)!;

            var shapes = idPointShapes.ToList();
            shapes.Sort((shape, pointShape) => shape.GetAnchor().GetDistance(hitMedia.Pos)
                .CompareTo(pointShape.GetAnchor().GetDistance(hitMedia.Pos)));
            var pointShapes = shapes.Take(num).Select(box => box.IdPointShape).OfType<ICanBeAndNeedHit>();
#if DEBUG
            var i = shapes.Count();
            var count = pointShapes.Count();
            Console.Out.WriteLine($"hit some body {i} num {num} after {count}");
#endif
            var relationMsgS =
                pointShapes.Select(hitMedia.HitSth).Where(x => x != null);

            return relationMsgS!;
        }
    }
}