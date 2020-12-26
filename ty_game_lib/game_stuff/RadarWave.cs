using System;
using System.Collections.Generic;
using System.IO;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class RadarWave : IHitMedia
    {
        public RadarWave(Dictionary<BodySize, BulletBox> sizeToBulletCollision)
        {
            TargetType = ObjType.OtherTeam;
            SizeToBulletCollision = sizeToBulletCollision;
            RdZone = GameTools.GenRdBox(sizeToBulletCollision);
            Pos = TwoDPoint.Zero();
            Aim = TwoDVector.Zero();
            Caster = null;
        }


        public static RadarWave GenByConfig(radar_wave radarWave)
        {
            var dictionary = GameTools.GenBulletShapes(radarWave.ShapeParams, radarWave.LocalRotate, radarWave.LocalPos,
                radarWave.ShapeType);
            return new RadarWave(dictionary);
        }

        public static RadarWave GenById(string id)
        {
            if (LocalConfig.Configs.radar_waves.TryGetValue(id, out var radarWave))
            {
                return GenByConfig(radarWave);
            }

            throw new DirectoryNotFoundException($"not such radar_wave id::{id}");
        }

        public Zone RdZone { get; }
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }

        public IPosMedia Active(TwoDPoint casterPos, TwoDVector casterAim)
        {
            return
                PosMediaStandard.Active(casterPos, casterAim, this);
        }

        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        public IBattleUnitStatus? Caster { get; set; }

        public void Sign(IBattleUnitStatus characterStatus)
        {
            Caster = characterStatus;
        }

        public HitResult? IsHitBody(IIdPointShape targetBody)
        {
            switch (targetBody)
            {
                case ICanBeHit characterBody1:


                    if (Caster == null || !IsHit(characterBody1)) return null;
                    if (Caster is Trap trap) trap.StartTrick();
                    Caster.GetMayBeSomeThing().Add(targetBody.GetAnchor());
// #if DEBUG
//                     Console.Out.WriteLine($"bullet hit::{isHit}");
// #endif
                    return new HitResult(characterBody1, false, Caster.GetFinalCaster(), this);


                default:
                    throw new ArgumentOutOfRangeException(nameof(targetBody));
            }
        }

        public IEnumerable<HitResult> HitTeam(IQSpace qSpace)
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