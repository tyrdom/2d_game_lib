using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class Radar : IHitMedia
    {
        public Radar(ObjType targetType, Dictionary<BodySize, BulletBox> sizeToBulletCollision, Zone rdZone,
            TwoDPoint pos, TwoDVector aim)
        {
            TargetType = targetType;
            SizeToBulletCollision = sizeToBulletCollision;
            RdZone = rdZone;
            Pos = pos;
            Aim = aim;
        }

        public Zone RdZone { get; }
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }
        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        public CharacterStatus? Caster { get; set; }

        public bool IsHitBody(IIdPointShape targetBody)
        {
            switch (targetBody)
            {
                case CharacterBody characterBody1:


                    if (Caster == null || !IsHit(characterBody1)) return false;
                    Caster.MayBeSomeThing.Add(targetBody.GetAnchor());
                    return true;
                case Trap trap:
                   
                    if (Caster == null || !IsHit(trap)) return false;
                    Caster.MayBeSomeThing.Add(targetBody.GetAnchor());
                    return true;

                // #if DEBUG
//                     Console.Out.WriteLine($"bullet hit::{isHit}");
// #endif
                
                
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