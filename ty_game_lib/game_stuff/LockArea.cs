using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    //在技能开始时，如果技能有锁定区域，那么可以获得到一个锁定
    public class LockArea : IHitMedia
    {
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }

        public bool IsHit(ICanBeHit characterBody, SightMap? blockMap)
        {
            return IsHit(characterBody);
        }

        public Dictionary<size, BulletBox> SizeToBulletCollision { get; }
        public IBattleUnitStatus? Caster { get; set; }

        public void Sign(IBattleUnitStatus characterStatus)
        {
            Caster = characterStatus;
        }

        public Zone RdZone { get; }

        private LockArea(Dictionary<size, BulletBox> sizeToBulletCollision)
        {
            Pos = TwoDPoint.Zero();
            Aim = TwoDVector.Zero();
            SizeToBulletCollision = sizeToBulletCollision;
            Caster = null;
            RdZone = GameTools.GenRdBox(sizeToBulletCollision);
        }

        public static bool TryGenById(string id, out LockArea? lockArea)
        {
            var enumType = typeof(lock_area_id);
            var isDefined = Enum.IsDefined(enumType, id);
            if (isDefined)
            {
                var lockAreaId = (lock_area_id) Enum.Parse(enumType, id);
                return TryGenById(lockAreaId, out lockArea);
            }

            lockArea = null;
            return false;
        }

        public static bool TryGenById(lock_area_id id, out LockArea aLockArea)
        {
            var configsLockAreas = CommonConfig.Configs.lock_areas;
            var tryGetValue = configsLockAreas.TryGetValue(id, out var lockArea);
            aLockArea = GenByConfig(lockArea);
            return tryGetValue;
        }

        public static LockArea GenByConfig(lock_area lockArea)
        {
            var genBulletShapes = GameTools.GenBulletShapes(lockArea.ShapeParams, lockArea.LocalRotate,
                lockArea.LocalPos, lockArea.ShapeType);

            return new LockArea(genBulletShapes);
        }

        public IEnumerable<IRelationMsg> HitTeam(IEnumerable<IQSpace> qSpaces, SightMap? blockMap)
        {
            return HitAbleMediaStandard.HitTeam(qSpaces, this, blockMap);
        }

        public ObjType TargetType => ObjType.OtherTeam;

        public bool HitNumLimit(out int num)
        {
            num = 0;
            return false;
        }


        public bool CanGoNextTick()
        {
            return false;
        }

        private bool IsHit(ICanBeHit characterBody)
        {
            return GameTools.IsHit(this, characterBody);
        }

        public IRelationMsg? HitSth(ICanBeHit canBeHit)
        {
            if (!(Caster is CharacterStatus characterStatus)) return null;
            switch (canBeHit)
            {
                case CharacterBody characterBody1:

                    characterStatus.LockingWho = characterBody1.CharacterStatus;
                    return new LockHit(characterBody1, Caster.GetFinalCaster().CharacterStatus, this);
                case Trap trap:
                    return new LockHit(trap, Caster.GetFinalCaster().CharacterStatus, this);
                default:
                    throw new ArgumentOutOfRangeException(nameof(canBeHit));
            }
        }

        public IPosMedia Active(TwoDPoint casterPos, TwoDVector casterAim)
        {
            return
                PosMediaStandard.Active(casterPos, casterAim, this);
        }
    }

    public class LockHit : IHitMsg
    {
        public LockHit(ICanBeHit whoTake, CharacterStatus casterOrOwner, IHitMedia lockArea1)
        {
            CasterOrOwner = casterOrOwner;
            WhoTake = whoTake;
            HitMedia = lockArea1;
        }

        public CharacterStatus CasterOrOwner { get; }
        public ICanBeHit WhoTake { get; }

        public IHitMedia HitMedia { get; }
    }
}