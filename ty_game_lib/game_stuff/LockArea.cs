using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    //在技能开始时，如果技能有锁定区域，那么可以获得到一个锁定
    public class LockArea : IEffectMedia
    {
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }
        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        public CharacterStatus? Caster { get; set; }

        public Zone RdZone { get; }

        private LockArea(Dictionary<BodySize, BulletBox> sizeToBulletCollision)
        {
            Pos = TwoDPoint.Zero();
            Aim = TwoDVector.Zero();
            SizeToBulletCollision = sizeToBulletCollision;
            Caster = null;
            RdZone = GameTools.GenRdBox(sizeToBulletCollision);
        }

        public static LockArea GenByConfig(game_config.lock_area lockArea)
        {
            var genBulletShapes = GameTools.GenBulletShapes(lockArea.ShapeParams, lockArea.LocalRotate,
                lockArea.LocalPos, lockArea.ShapeType);

            return new LockArea(genBulletShapes);
        }

        public ObjType TargetType => ObjType.OtherTeam;

        public bool CanGoNextTick()
        {
            return false;
        }

        public bool IsHit(CharacterBody characterBody)
        {
            return GameTools.IsHit(this, characterBody);
        }

        private bool HitBody(IIdPointShape targetBody)
        {
            switch (targetBody)
            {
                case CharacterBody characterBody1:
                    var isHit = IsHit(characterBody1);
                    if (isHit && Caster != null)
                    {
                        Caster.LockingWho =
                            characterBody1.CharacterStatus;
                    }

                    return isHit;

                default:
                    throw new ArgumentOutOfRangeException(nameof(targetBody));
            }
        }

        public LockArea ActiveArea(TwoDPoint casterPos, TwoDVector casterAim)
        {
            Pos = casterPos;
            Aim = casterAim;
            return this;
        }

        public HashSet<int> HitTeam(IQSpace qSpace)
        {
            var mapToGidList = qSpace.FilterToGIdPsList((body, bullet) => bullet.HitBody(body),
                this, RdZone.MoveToAnchor(Pos));
            return SomeTools.EnumerableToHashSet(mapToGidList.Select(x => x.GetId()));
        }
    }
}