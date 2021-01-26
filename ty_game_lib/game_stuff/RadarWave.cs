using System;
using System.Collections.Generic;
using System.IO;
using collision_and_rigid;
using Force.DeepCloner;
using game_config;

namespace game_stuff
{
    public class RadarSeeMsg : ISeeTickMsg
    {
        public RadarSeeMsg(TwoDPoint pos, BodySize size)
        {
            Pos = pos;
            Size = size;
        }

        public TwoDPoint Pos { get; }

        public BodySize Size { get; }
    }

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

        public IRelationMsg? IsHitBody(IIdPointShape targetBody)
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
                    return new RadarHit(characterBody1, Caster.GetFinalCaster().CharacterStatus, this);


                default:
                    throw new ArgumentOutOfRangeException(nameof(targetBody));
            }
        }

        public IEnumerable<IRelationMsg> HitTeam(IQSpace qSpace)
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

    public class RadarHit : IRelationMsg
    {
        public RadarHit(ICanBeHit whoTake, CharacterStatus casterOrOwner, RadarWave radarWave)
        {
            WhoTake = whoTake;
            CasterOrOwner = casterOrOwner;
            RadarWave = radarWave;
        }

        public CharacterStatus CasterOrOwner { get; }
        public ICanBeHit WhoTake { get; }
        public RadarWave RadarWave { get; }
    }
}