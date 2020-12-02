﻿using System;
using collision_and_rigid;

namespace game_stuff
{
    public class CharTickMsg : ISeeTickMsg
    {
        public int Gid;
        public TwoDPoint Pos;
        public TwoDVector Aim;
        public float Speed;
        public SurvivalStatus SurvivalStatus;
        public bool IsPause;
        public float SightR;
        public SkillAction? SkillLaunch;
        public bool SkillAct;
        public bool IsStun;
        public TwoDVector? IsBeHit;
        public bool IsHitSome;

        public CharTickMsg(int gid, TwoDPoint pos, TwoDVector aim, SurvivalStatus survivalStatus,
            SkillAction? skillLaunch,
            bool antiBuff, float speed, float sightR, bool isPause, bool skillAct, TwoDVector? isBeHit, bool isHitSome)
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
        }
    }
}