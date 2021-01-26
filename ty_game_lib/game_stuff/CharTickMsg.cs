using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public interface IBodyChange
    {
    }

    public readonly struct MovePosAim : IBodyChange
    {
        public TwoDPoint? Pos { get; }
        public TwoDVector? Aim { get; }
        public float Speed { get; }
    }


    public readonly struct StunPosAim : IBodyChange
    {
        public TwoDPoint? Pos { get; }
        public TwoDVector? Aim { get; }
    }

    public readonly struct ActPos : IBodyChange
    {
        public TwoDPoint? Pos { get; }
        public TwoDVector? Aim { get; }
    }


    public readonly struct SurvivalStatusChange
    {
        public uint? Hp { get; }
        public uint? Ammo { get; }
        public uint? Shield { get; }
        public uint? Armor { get; }
        public uint? Prop { get; }
    }

    public readonly struct SkillLaunch
    {
        public SkillAction SkillAction { get; }

        public int CIndex { get; }
    }

    public readonly struct SpecialActionLaunch
    {
        public SpecialAction SpecialAction { get; }
    }

    public readonly struct MapLaunch
    {
        public MapInteract MapAction { get; }
    }

    public readonly struct SightChange
    {
        private float SightR { get; }
    }

    public readonly struct AimSightChange
    {
    }

    public readonly struct CharTickMsg : ISeeTickMsg
    {
        public int Gid { get; }
        public TwoDPoint Pos { get; }
        public TwoDVector Aim { get; }
        public float Speed { get; }
        public SurvivalStatus SurvivalStatus { get; }
        public bool IsPause { get; }
        public float SightR { get; }
        public SkillAction? SkillLaunch { get; }
        public bool SkillOnAct { get; }
        public bool IsStun { get; }
        public TwoDVector? IsBeHit { get; }
        public bool IsHitSome { get; }

        public CharTickMsg(int gid, TwoDPoint pos, TwoDVector aim, SurvivalStatus survivalStatus,
            SkillAction? skillLaunch,
            bool antiBuff, float speed, float sightR, bool isPause, bool skillOnAct, TwoDVector? isBeHit,
            bool isHitSome)
        {
            Gid = gid;
            Pos = pos;
            Aim = aim;
            SurvivalStatus = survivalStatus;
            SkillLaunch = skillLaunch;
            IsStun = antiBuff;
            Speed = speed;
            SightR = sightR;
            IsPause = isPause;
            SkillOnAct = skillOnAct;
            IsBeHit = isBeHit;
            IsHitSome = isHitSome;
        }

        public override string ToString()
        {
            return
                $"gid: {Gid} Pos: {Pos} Aim {Aim} SightR {SightR} \n {SurvivalStatus}\n" +
                $" is on hit::{IsBeHit} , is stun :: {IsStun},skill act {SkillOnAct} launch {SkillLaunch} IsHitSth{IsHitSome}";
        }
    }
}