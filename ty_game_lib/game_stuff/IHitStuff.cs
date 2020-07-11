using System.Collections.Generic;
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
}