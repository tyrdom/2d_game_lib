using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class CharacterBody : IIdPointShape
    {
        public BodySize BodySize;
        public CharacterStatus CharacterStatus;
        public TwoDPoint LastPos;

        public TwoDPoint NowPos;
        public AngleSight Sight;
        public int Team;

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
            return Sight.InSight(new TwoDVectorLine(NowPos, another.GetAnchor()), map);
        }

        public AabbBoxShape CovToAabbPackBox()
        {
            var zone = new Zone(0f, 0f, 0f, 0f);
            return new AabbBoxShape(zone, this);
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
            LastPos = NowPos;
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

        public (ITwoDTwoP?, IHitStuff?) BodyGoATick(Dictionary<int, Operate> gidToOp)
        {
            var id = GetId();
            if (!gidToOp.TryGetValue(id, out var o)) return CharacterStatus.CharGoTick(null);
            var charGoTick = CharacterStatus.CharGoTick(o);

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
            var type = CharacterStatus.AntiActBuff?.GetType();
            return new CharTickMsg(GetId(), NowPos, Sight.Aim, CharacterStatus.DamageHealStatus,
                CharacterStatus.NowCastSkill != null, type, CharacterStatus.NowMoveSpeed);
        }

        public CharInitMsg GenInitMsg()
        {
            return new CharInitMsg(GetId(), NowPos, Sight.Aim, CharacterStatus.DamageHealStatus,
                CharacterStatus.Weapons);
        }
    }
}