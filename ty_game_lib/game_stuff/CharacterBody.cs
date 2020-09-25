using System;
using System.Collections.Generic;
using System.ComponentModel;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class CharacterBody : IIdPointShape
    {
        public BodySize BodySize
        {
            get => CharacterStatus.NowVehicle?.VehicleSize ?? BodySize;
            private set
            {
                if (!Enum.IsDefined(typeof(BodySize), value))
                    throw new InvalidEnumArgumentException(nameof(value), (int) value, typeof(BodySize));
                BodySize = value;
            }
        }

        public CharacterStatus CharacterStatus { get; }
        public TwoDPoint LastPos { get; private set; }

        public TwoDPoint NowPos { get; private set; }
        public AngleSight Sight { get; }
        public int Team { get; }
        private IdPointBox? IdPointBox { get; set; }


        public CharacterBody(TwoDPoint nowPos, BodySize bodySize, CharacterStatus characterStatus,
            TwoDPoint lastPos,
            AngleSight sight, int team)
        {
            NowPos = nowPos;
            BodySize = bodySize;
            characterStatus.CharacterBody = this;
            CharacterStatus = characterStatus;
            LastPos = lastPos;
            Sight = sight;
            Team = team;
        }

        public bool InSight(IIdPointShape another, SightMap map)
        {
            return Sight.InSight(new TwoDVectorLine(NowPos, another.GetAnchor()), map, CharacterStatus.GetNowScope());
        }

        public IdPointBox CovToAaBbPackBox()
        {
            if (IdPointBox != null) return IdPointBox;
            var zone = new Zone(0f, 0f, 0f, 0f);
            var covToAaBbPackBox = new IdPointBox(zone, this);
            IdPointBox = covToAaBbPackBox;
            return covToAaBbPackBox;
        }


        public float GetRr()
        {
            return TempConfig.SizeToR.TryGetValue(BodySize, out var valueOrDefault) ? valueOrDefault : 1f;
        }

        public int GetId()
        {
            return CharacterStatus.GId;
        }

        public TwoDPoint Move(ITwoDTwoP vector)
        {
            // LastPos = NowPos;
            var twoDPoint = vector switch
            {
                TwoDPoint twoDPoint1 => twoDPoint1,
                TwoDVector twoDVector => NowPos.Move(twoDVector),
                _ => throw new ArgumentOutOfRangeException(nameof(vector))
            };

            NowPos = twoDPoint;
            return NowPos;
        }

        public TwoDPoint GetAnchor()
        {
            return NowPos;
        }

        public TwoDVectorLine GetMoveVectorLine()
        {
            return new TwoDVectorLine(LastPos, NowPos);
        }

        public ITwoDTwoP RelocateWithBlock(WalkBlock walkBlock)
        {
            var (isHitWall, pt) =
                walkBlock.PushOutToPt(LastPos, NowPos);

#if DEBUG
            // if (walkBlock.QSpace != null)
            //     Console.Out.WriteLine(
            //         $" check:: {qSpace.Count()} map :: shapes num {walkBlock.QSpace.Count()}");
            // Console.Out.WriteLine(
            //     $" lastPos:: {characterBody.LastPos.Log()} nowPos::{characterBody.NowPos.Log()}");
#endif
            if (isHitWall) HitWall();
            // var coverPoint = walkBlock.RealCoverPoint(pt);
            // if (coverPoint) pt = LastPos;

            return pt;
        }


        public CharGoTickMsg BodyGoATick(Dictionary<int, Operate> gidToOp)
        {
            LastPos = NowPos;
            var id = GetId();
            if (!gidToOp.TryGetValue(id, out var o)) return CharacterStatus.CharGoTick(null);
            var charGoTick = CharacterStatus.CharGoTick(o);
#if DEBUG
            Console.Out.WriteLine($"bgt::{charGoTick.Move?.ToString()}");
#endif
            return charGoTick;
        }

        public void HitWall()
        {
            var characterStatusAntiActBuff = CharacterStatus.AntiActBuff;
            if (characterStatusAntiActBuff == null)
            {
                return;
            }

            var hitWall = characterStatusAntiActBuff.HitWall();
            var hitWallDmgParam = 1 + (int) (TempConfig.HitWallDmgParam * hitWall);
            CharacterStatus.DamageHealStatus.TakeDamage(new Damage(hitWallDmgParam));
        }

        public CharTickMsg GenTickMsg()
        {
            var isStun = CharacterStatus.AntiActBuff != null;
            var skillAct = CharacterStatus.NowCastAct != null;
            var characterStatusIsOnHitBySomeOne = CharacterStatus.IsBeHitBySomeOne;
            return new CharTickMsg(GetId(), NowPos, Sight.Aim, CharacterStatus.DamageHealStatus,
                CharacterStatus.SkillLaunch, isStun, CharacterStatus.NowMoveSpeed, Sight.NowR,
                CharacterStatus.IsPause,
                skillAct, characterStatusIsOnHitBySomeOne, CharacterStatus.IsHitSome);
        }

        public CharInitMsg GenInitMsg()
        {
            return new CharInitMsg(GetId(), NowPos, Sight.Aim, CharacterStatus.DamageHealStatus,
                CharacterStatus.Weapons);
        }

        public override string ToString()
        {
            return $"Id {GetId()} pos {NowPos}";
        }

        public void Renew(CharacterInitData characterInitData)
        {
            characterInitData.ReloadCharacterBody(this);
        }
    }
}