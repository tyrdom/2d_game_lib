using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class CharacterBody : ICanBeAndNeedHit
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

        public (UnitType unitType, int gid) GetTypeAndId()
        {
            return (UnitType.CharacterBody, GetId());
        }

        public CharacterStatus CharacterStatus { get; }
        private TwoDPoint LastPos { get; set; }
        public TwoDPoint NowPos { get; private set; }
        public AngleSight Sight { get; }
        public int Team { get; }
        public IdPointBox IdPointBox { get; set; }

        public int GetTeam()
        {
            return Team;
        }


        public CharacterBody(TwoDPoint nowPos, size bodySize, CharacterStatus characterStatus,
            TwoDPoint lastPos,
            AngleSight sight, int team)
        {
            var zone = Zone.Zero();
            var covToAaBbPackBox = new IdPointBox(zone, this);
            IdPointBox = covToAaBbPackBox;
            NowPos = nowPos;
            BodySize = bodySize;
            characterStatus.CharacterBody = this;
            CharacterStatus = characterStatus;
            LastPos = lastPos;
            Sight = sight;
            Team = team;
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
#if DEBUG
            if (GetId() == 1)
            {
                Console.Out.WriteLine($"pos {NowPos} want see {another.GetAnchor()}");
            }
#endif
            return Sight.InSight(new TwoDVectorLine(NowPos, another.GetAnchor()), map, CharacterStatus.GetNowScope());
        }

        public bool Hear(Bullet bullet, SightMap? map)
        {
            return CharacterStatus.Hear(bullet, map);
        }


        public IBattleUnitStatus GetBattleUnitStatus()
        {
            return CharacterStatus;
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
            CharacterStatus.NewPt();
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
            if (NowPos.Same(LastPos))
            {
                return null;
            }

            var (isHitWall, pt) =
                walkBlock.PushOutToPt(LastPos, NowPos, out var dVector);

#if DEBUG
            // Console.Out.WriteLine(
            //     $" check:: {walkBlock.QSpace.Count()} map :: shapes num {walkBlock.QSpace.Count()}");
            // Console.Out.WriteLine(
            //     $" lastPos:: {LastPos} nowPos::{NowPos} :: is hit wall ::{isHitWall}");
#endif

            var realCoverPoint = walkBlock.RealCoverPoint(pt);
            if (realCoverPoint)
            {
                pt = LastPos;
            }

            if (!isHitWall && !realCoverPoint)
            {
                return (NowPos, null);
            }

            NowPos = pt;
            var characterStatusStunBuff = CharacterStatus.StunBuff;

            if (characterStatusStunBuff == null)
            {
                return (pt, null);
            }
            
            var hitWall = characterStatusStunBuff.HitWall();

            var takeDamage = CharacterStatus.TakeDamage(hitWall);
            if (takeDamage == null)
            {
                return (pt, null);
            }

            var dmgShow = takeDamage.Value;
            var dmgShowHarmResults = dmgShow.HarmResults;
            var hitMark = new HitMark(dVector, bullet_id.default_block_1, dmgShowHarmResults);
            CharacterStatus.CharEvents.Add(hitMark);
            var buffDmgMsg = new BuffDmgMsg(characterStatusStunBuff.Caster.GetFinalCaster().CharacterStatus,
                dmgShow, this);
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
            return GenCharTickMsg();
        }

        public CharTickMsg GenCharTickMsg(int? gid = null)
        {
            var characterStatusCharEvents = CharacterStatus.CharEvents;
            var gameItems = characterStatusCharEvents.OfType<ItemChange>().SelectMany(x => x.ItemNow).IeToHashSet()
                .Select(
                    x => CharacterStatus.PlayingItemBag.GetNum(x)
                ).ToArray();
            if (gameItems.Any())
            {
#if DEBUG
                Console.Out.WriteLine("");
#endif
                var itemDetailChange = new ItemDetailChange(gameItems);
                characterStatusCharEvents.Add(itemDetailChange);
            }

            characterStatusCharEvents.UnionWith(CharacterStatus.GenBaseChangeMarksEvents());
            characterStatusCharEvents.UnionWith(CharacterStatus.GetNowSurvivalStatus().GenSurvivalEvents());

            return new CharTickMsg(GetId(), characterStatusCharEvents);
        }

        public CharInitMsg GenInitMsg()
        {
            var weaponConfigs = CharacterStatus.GetWeapons()[CharacterStatus.NowWeapon];
            return new CharInitMsg(GetId(), NowPos, Sight.Aim, CharacterStatus.SurvivalStatus,
                weaponConfigs, CharacterStatus.PassiveTraits,
                CharacterStatus.PlayingItemBag, CharacterStatus.Prop);
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

        public void MakeProtect(int tick)
        {
            CharacterStatus.MakeProtect(tick);
        }

        public bool AvoidWave(int key)
        {
            return CharacterStatus.AvoidWave(key);
        }
    }


    public readonly struct BuffDmgMsg : IDamageMsg
    {
        public BuffDmgMsg(CharacterStatus casterOrOwner, DmgShow dmgShow, ICanBeAndNeedHit whoTake)
        {
            CasterOrOwner = casterOrOwner;
            DmgShow = dmgShow;
            WhoTake = whoTake;
        }

        public CharacterStatus CasterOrOwner { get; }


        public DmgShow DmgShow { get; }

        public ICanBeAndNeedHit WhoTake { get; }
    }

    public readonly struct DmgShow
    {
        public DmgShow(bool isKill, int[] harmResults)
        {
            IsKill = isKill;
            HarmResults = harmResults;
        }

        public bool IsKill { get; }
        public int[] HarmResults { get; }
    }
}