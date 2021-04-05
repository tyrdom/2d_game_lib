using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class CharacterBody : ICanBeHit
    {
        public bool RealDead()
        {
            return CharacterStatus.StunBuff == null && CharacterStatus.SurvivalStatus.IsDead();
        }

        public TwoDVector GetAim()
        {
            return Sight.Aim;
        }

        public size BodySize { get; }

        public size GetSize()
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

        public int GetTeam()
        {
            return Team;
        }

        public IdPointBox InBox { get; set; }


        public CharacterBody(TwoDPoint nowPos, size bodySize, CharacterStatus characterStatus,
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

        public bool InSight(IHaveAnchor another, SightMap? map)
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
            return StuffLocalConfig.GetRBySize(BodySize);
        }

        public int GetId()
        {
            return CharacterStatus.GId;
        }

        public TwoDPoint Move(ITwoDTwoP vector)
        {
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

        public (ITwoDTwoP pt, BuffDmgMsg? buffDmgMsg)? RelocateWithBlock(WalkBlock walkBlock)
        {
            var (isHitWall, pt) =
                walkBlock.PushOutToPt(LastPos, NowPos);

#if DEBUG
            // Console.Out.WriteLine(
            //     $" check:: {walkBlock.QSpace.Count()} map :: shapes num {walkBlock.QSpace.Count()}");
            // Console.Out.WriteLine(
            //     $" lastPos:: {LastPos} nowPos::{NowPos} :: is hit wall ::{isHitWall}");
#endif
            if (!isHitWall) return null;
            if (CharacterStatus.StunBuff == null)
            {
                return (pt, null);
            }

            var hitWall = CharacterStatus.StunBuff.HitWall();

            var takeDamage = CharacterStatus.TakeDamage(hitWall);
            if (takeDamage == null)
            {
                return (pt, null);
            }

            var buffDmgMsg = new BuffDmgMsg(CharacterStatus.StunBuff.Caster.GetFinalCaster().CharacterStatus,
                takeDamage.Value, this);
            return (pt, buffDmgMsg);
        }


        public CharGoTickResult GoATick(Dictionary<int, Operate> gidToOp)
        {
            LastPos = NowPos;
            var id = GetId();
            if (!gidToOp.TryGetValue(id, out var o)) return CharacterStatus.CharGoTick(null);
            var charGoTick = CharacterStatus.CharGoTick(o);
#if DEBUG
            // Console.Out.WriteLine($"bgt::{charGoTick.Move?.ToString()}");
#endif
            return charGoTick;
        }


        public ISeeTickMsg GenTickMsg(int? gid = null)
        {
            var isStun = CharacterStatus.StunBuff != null;
            var skillAct = ((int?) CharacterStatus.NowCastAct?.NowOnTick ?? -1);
            var characterStatusIsOnHitBySomeOne = CharacterStatus.IsBeHitBySomeOne;
            var sightStandardScope = CharacterStatus.GetNowScope() ?? Sight.StandardScope;
            return new CharTickMsg(GetId(), NowPos, Sight.Aim, CharacterStatus.SurvivalStatus,
                CharacterStatus.SkillLaunch, isStun, CharacterStatus.NowMoveSpeed, Sight.NowR,
                CharacterStatus.IsPause,
                skillAct, characterStatusIsOnHitBySomeOne, CharacterStatus.IsHitSome
                , sightStandardScope.Theta
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
    }

    public readonly struct BuffDmgMsg : IDamageMsg
    {
        public BuffDmgMsg(CharacterStatus casterOrOwner, DmgShow dmgShow, ICanBeHit whoTake)
        {
            CasterOrOwner = casterOrOwner;
            DmgShow = dmgShow;
            WhoTake = whoTake;
        }

        public CharacterStatus CasterOrOwner { get; }


        public DmgShow DmgShow { get; }

        public ICanBeHit WhoTake { get; }
    }

    public readonly struct DmgShow
    {
        public DmgShow(bool isKill, Damage damage)
        {
            IsKill = isKill;
            Damage = damage;
        }

        public bool IsKill { get; }
        public Damage Damage { get; }
    }
}