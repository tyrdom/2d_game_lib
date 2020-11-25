using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class RadarWave : IHitMedia
    {
        private CharacterStatus? Caster1;

        public RadarWave(ObjType targetType, Dictionary<BodySize, BulletBox> sizeToBulletCollision, Zone rdZone,
            TwoDPoint pos, TwoDVector aim, IBattleUnitStatus? caster)
        {
            TargetType = targetType;
            SizeToBulletCollision = sizeToBulletCollision;
            RdZone = rdZone;
            Pos = pos;
            Aim = aim;
            Caster = caster;
        }

        public Zone RdZone { get; }
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }
        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        public IBattleUnitStatus? Caster { get; set; }

        public bool IsHitBody(IIdPointShape targetBody)
        {
            switch (targetBody)
            {
                case ICanBeHit characterBody1:


                    if (Caster == null || !IsHit(characterBody1)) return false;
                    Caster.GetMayBeSomeThing()?.Add(targetBody.GetAnchor());
// #if DEBUG
//                     Console.Out.WriteLine($"bullet hit::{isHit}");
// #endif
                    return true;


                default:
                    throw new ArgumentOutOfRangeException(nameof(targetBody));
            }
        }

        public HashSet<int> HitTeam(IQSpace qSpace)
        {
            return HitAbleMediaStandard.HitTeam(qSpace, this);
        }

        public ObjType TargetType { get; }

        public bool CanGoNextTick()
        {
            return false;
        }


        public bool IsHit(ICanBeHit characterBody)
        {
            return GameTools.IsHit(this, characterBody);
        }
    }
}