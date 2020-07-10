using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    interface IHitStuff
    {
        public bool IsActive { get; set; }
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }
        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        public CharacterStatus Caster { get; }
    }

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
    }
}