using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public interface IHitStuff
    {
        public bool IsActive { get; set; }
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }
        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        public CharacterStatus Caster { get; }

        public bool IsHit(CharacterBody characterBody);
    }


    //在技能开始时，如果技能有锁定区域，那么可以获得到一个锁定
    public class LockArea : IHitStuff
    {
        public bool IsActive { get; set; }
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }
        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        public CharacterStatus Caster { get; }

        public LockArea(bool isActive, TwoDPoint pos, TwoDVector aim,
            Dictionary<BodySize, BulletBox> sizeToBulletCollision, CharacterStatus caster)
        {
            IsActive = isActive;
            Pos = pos;
            Aim = aim;
            SizeToBulletCollision = sizeToBulletCollision;
            Caster = caster;
        }

        public bool IsHit(CharacterBody characterBody)
        {
         return   GameTools.IsHit(this, characterBody);
        }
        
        public bool HitBody(IIdPointShape targetBody)
        {
            switch (targetBody)
            {
                case CharacterBody characterBody1:
                    var isHit = IsHit(characterBody1);
                    if (isHit)
                    {
                        //todo 释放者锁定目标
                    }

                    return isHit;

                default:
                    throw new ArgumentOutOfRangeException(nameof(targetBody));
            }
        }
        public HashSet<int> HitTeam(IQSpace qSpace)
        {
            var mapToGidList = qSpace.FilterToGIdPsList((body, bullet) => bullet.HitBody(body),
                this);
            return SomeTools.ListToHashSet(mapToGidList.Select(x => x.GetId()));
        }
    }
}