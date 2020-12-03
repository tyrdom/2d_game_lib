using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class CharTickMsg : ISeeTickMsg
    {
        public readonly int Gid;
        public readonly TwoDPoint Pos;
        public readonly TwoDVector Aim;
        public readonly float Speed;
        public readonly SurvivalStatus SurvivalStatus;
        public readonly bool IsPause;
        public readonly float SightR;
        public readonly SkillAction? SkillLaunch;
        public readonly bool SkillAct;
        public readonly bool IsStun;
        public readonly TwoDVector? IsBeHit;
        public readonly bool IsHitSome;
        public readonly List<TwoDPoint> SomeThings;

        public CharTickMsg(int gid, TwoDPoint pos, TwoDVector aim, SurvivalStatus survivalStatus,
            SkillAction? skillLaunch,
            bool antiBuff, float speed, float sightR, bool isPause, bool skillAct, TwoDVector? isBeHit, bool isHitSome,
            List<TwoDPoint> someThings)
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
            SkillAct = skillAct;
            IsBeHit = isBeHit;
            IsHitSome = isHitSome;
            SomeThings = someThings;
        }

        public override string ToString()
        {
            return
                $"gid: {Gid} Pos: {Pos} Aim {Aim} SightR {SightR} \n {SurvivalStatus}\n" +
                $" is on hit::{IsBeHit} , is stun :: {IsStun},skill act {SkillAct} launch {SkillLaunch} IsHitSth{IsHitSome}";
        }
    }
}