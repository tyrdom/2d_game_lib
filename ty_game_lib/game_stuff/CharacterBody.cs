using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class CharacterBody : ICanBeHit
    {
        private BodySize BodySize { get; }

        public BodySize GetSize()
        {
            return CharacterStatus.NowVehicle?.Size ?? BodySize;
        }

        public bool CheckCanBeHit()
        {
            return CharacterStatus.CheckCanBeHit();
        }

        public CharacterStatus CharacterStatus { get; }
        private TwoDPoint LastPos { get; set; }
        public TwoDPoint NowPos { get; private set; }
        public AngleSight Sight { get; }
        public int Team { get; }
        public IdPointBox? IdPointBox { get; set; }
        public IdPointBox InBox { get; set; }


        // public static CharacterBody GenByCharId(int id)
        // {
        // }
        //
        // public static CharacterBody GenByCreeperId(int id)
        // {
        // }

        public CharacterBody(TwoDPoint nowPos, BodySize bodySize, CharacterStatus characterStatus,
            TwoDPoint lastPos,
            AngleSight sight, int team)
        {
            IdPointBox = null;
            NowPos = nowPos;
            BodySize = bodySize;
            characterStatus.CharacterBody = this;
            CharacterStatus = characterStatus;
            LastPos = lastPos;
            Sight = sight;
            Team = team;

            InBox = CovToIdBox();
        }

        public void Teleport(TwoDPoint twoDPoint)
        {
            NowPos = twoDPoint;
            LastPos = twoDPoint;
        }

        public Zone GetSightZone()
        {
            return Sight.GenZone(GetAnchor());
        }

        public bool InSight(IHaveAnchor another, SightMap map)
        {
            return Sight.InSight(new TwoDVectorLine(NowPos, another.GetAnchor()), map, CharacterStatus.GetNowScope());
        }

        public IdPointBox CovToIdBox()
        {
            if (IdPointBox != null) return IdPointBox;
            var zone = Zone.Zero();
            var covToAaBbPackBox = new IdPointBox(zone, this);
            IdPointBox = covToAaBbPackBox;
            return covToAaBbPackBox;
        }


        public float GetRr()
        {
            return LocalConfig.SizeToR.TryGetValue(BodySize, out var valueOrDefault) ? valueOrDefault : 1f;
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


        public CharGoTickResult GoATick(Dictionary<int, Operate> gidToOp)
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

        private void HitWall()
        {
            var characterStatusAntiActBuff = CharacterStatus.StunBuff;
            if (characterStatusAntiActBuff == null)
            {
                return;
            }

            var hitWall = characterStatusAntiActBuff.HitWall();
            var hitWallDmgParam = 1 + (uint) (LocalConfig.HitWallDmgParam * hitWall);
            CharacterStatus.SurvivalStatus.TakeOneDamage(hitWallDmgParam);
        }

        public ISeeTickMsg GenTickMsg(int? gid = null)
        {
            var isStun = CharacterStatus.StunBuff != null;
            var skillAct = CharacterStatus.NowCastAct != null;
            var characterStatusIsOnHitBySomeOne = CharacterStatus.IsBeHitBySomeOne;
            return new CharTickMsg(GetId(), NowPos, Sight.Aim, CharacterStatus.SurvivalStatus,
                CharacterStatus.SkillLaunch, isStun, CharacterStatus.NowMoveSpeed, Sight.NowR,
                CharacterStatus.IsPause,
                skillAct, characterStatusIsOnHitBySomeOne, CharacterStatus.IsHitSome,
                CharacterStatus.MayBeSomeThing.ToImmutableArray()
            );
        }

        public CharInitMsg GenInitMsg()
        {
            return new CharInitMsg(GetId(), NowPos, Sight.Aim, CharacterStatus.SurvivalStatus,
                CharacterStatus.GetWeapons());
        }

        public override string ToString()
        {
            return $"Id {GetId()} pos {NowPos}";
        }

        public bool Include(TwoDPoint pos)
        {
            return false;
        }

        public void ReBorn(TwoDPoint pos)
        {
            Teleport(pos);
            CharacterStatus.Reborn();
        }

        public LevelUpsData GetAutoRebornTick()
        {
            return CharacterStatus.GetNowLevelUpData();
        }
    }

    public enum BodyMark
    {
        Player,
        Creep,
        Boss
    }
}