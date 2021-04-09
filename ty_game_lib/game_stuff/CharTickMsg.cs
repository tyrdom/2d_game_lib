using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public readonly struct CharTickMsg : ISeeTickMsg
        //todo only change to set value
    {
        public int Gid { get; }
        public TwoDPoint Pos { get; }
        public TwoDVector Aim { get; }
        public float Speed { get; }
        public SurvivalStatus SurvivalStatus { get; }
        public bool IsPause { get; }
        public float SightR { get; }

        public float SightRad { get; }
        public SkillAction? SkillLaunch { get; }
        public SnipeAction? SnipeActionLaunch { get; }
        public int SkillOnAct { get; }
        public bool IsStun { get; }
        public TwoDVector? IsBeHit { get; }
        public bool IsHitSome { get; }

        public bool SilentChangeWeapon { get; }
        public CharTickMsg(int gid, TwoDPoint pos, TwoDVector aim, SurvivalStatus survivalStatus,
            SkillAction? skillLaunch,
            bool antiBuff, float speed, float sightR, bool isPause, int skillOnAct, TwoDVector? isBeHit,
            bool isHitSome, float sightRad, SnipeAction? snipeActionLaunch, bool silentChangeWeapon)
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
            SightRad = sightRad;
            SnipeActionLaunch = snipeActionLaunch;
            SilentChangeWeapon = silentChangeWeapon;
        }

        public override string ToString()
        {
            return
                $"gid: {Gid} Pos: {Pos} Aim {Aim} SightR {SightR} \n {SurvivalStatus}\n" +
                $" is on hit::{IsBeHit} , is stun :: {IsStun},skill act {SkillOnAct} launch {SkillLaunch} IsHitSth{IsHitSome}";
        }
    }
}