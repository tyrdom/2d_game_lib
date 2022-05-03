using System;
using System.Collections.Generic;
using System.IO;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class RadarSee : ICanBeEnemy
    {
        public RadarSee(TwoDPoint pos, size size)
        {
            Pos = pos;
            Size = size;
        }

        private TwoDPoint Pos { get; }

        private size Size { get; }

        public TwoDPoint GetAnchor()
        {
            return Pos;
        }

        public float GetRr()
        {
           return StuffLocalConfig.GetRBySize(Size);
        }
    }

    public class RadarWave : IHitMedia
    {
        private RadarWave(Dictionary<size, BulletBox> sizeToBulletCollision)
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
            var o = (radar_wave_id) Enum.Parse(typeof(radar_wave_id), id, true);
            return GenById(o);
        }

        public static RadarWave GenById(radar_wave_id id)
        {
            if (CommonConfig.Configs.radar_waves.TryGetValue(id, out var radarWave))
            {
                return GenByConfig(radarWave);
            }

            throw new KeyNotFoundException($"not such radar_wave id::{id}");
        }

        public Zone RdZone { get; }
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }

        public IPosMedia Active(TwoDPoint casterPos, TwoDVector casterAim)
        {
            return
                PosMediaStandard.Active(casterPos, casterAim, this);
        }

        public bool IsCollisionHit(ICanBeAndNeedHit characterBody, SightMap? blockMap)
        {
            return IsHit(characterBody);
        }

        public Dictionary<size, BulletBox> SizeToBulletCollision { get; }
        public IBattleUnitStatus? Caster { get; set; }

        public void Sign(IBattleUnitStatus battleStatus)
        {
            Caster = battleStatus;
        }

        public IRelationMsg? HitSth(ICanBeAndNeedHit canBeAndNeedHit)
        {
            switch (Caster)
            {
                case null:
                    return null;
                case Trap trap:
                    trap.StartTrick();
                    break;
            }

// #if DEBUG
//                     Console.Out.WriteLine($"bullet hit::{isHit}");
// #endif
            return new RadarHit(canBeAndNeedHit, Caster.GetFinalCaster().CharacterStatus, this);
        }

        public IEnumerable<IRelationMsg> HitTeam(IEnumerable<IQSpace> qSpaces, SightMap? blockMap)
        {
            return HitAbleMediaStandard.HitTeam(qSpaces, this, blockMap);
        }

        public ObjType TargetType { get; }

        public bool HitNumLimit(out int num)
        {
            num = 0;
            return false;
        }


        public bool CanGoNextTick()
        {
            return false;
        }


        private bool IsHit(ICanBeAndNeedHit characterBody)
        {
            return GameTools.IsHit(this, characterBody);
        }
    }

    public class RadarHit : IHitMsg
    {
        public RadarHit(ICanBeAndNeedHit whoTake, CharacterStatus casterOrOwner, RadarWave hitMedia)
        {
            WhoTake = whoTake;
            CasterOrOwner = casterOrOwner;
            HitMedia = hitMedia;
        }

        public CharacterStatus CasterOrOwner { get; }
        public ICanBeAndNeedHit WhoTake { get; }
        public RadarWave HitMedia { get; }
    }
}