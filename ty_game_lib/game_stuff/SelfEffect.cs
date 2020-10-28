using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class SelfEffect : IHitStuff
    {
        public SelfEffect(CharacterStatus? caster)
        {
            RdZone = Zone.Zero();
            Pos = TwoDPoint.Zero();
            Aim = TwoDVector.Zero();
            SizeToBulletCollision = new Dictionary<BodySize, BulletBox>();
            Caster = caster;
            TargetType = ObjType.OnlyMyself;
        }

        public Zone RdZone { get; }
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }
        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        public CharacterStatus? Caster { get; set; }

        public HashSet<int> HitTeam(IQSpace qSpace)
        {
            return new HashSet<int>();
        }

        public ObjType TargetType { get; }

        public bool CanGoNextTick()
        {
            return false;
        }

        public bool IsHit(CharacterBody characterBody)
        {
            return false;
        }
    }
}