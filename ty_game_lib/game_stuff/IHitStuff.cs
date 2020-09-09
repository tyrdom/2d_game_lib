using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public interface IHitStuff
    {
        public Zone RdZone { get; }
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }
        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        public CharacterStatus? Caster { get; set; }
        public HashSet<int> HitTeam(IQSpace qSpace);
        public ObjType TargetType { get; }
        public bool CanGoNextTick();
        public bool IsHit(CharacterBody characterBody);
    }
}