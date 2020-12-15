using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using collision_and_rigid;

namespace game_stuff
{
    public class CharTickMsg : ISeeTickMsg
    {
        public int Gid { get; }
        public TwoDPoint Pos { get; }
        public TwoDVector Aim { get; }
        public float Speed { get; }
        public SurvivalStatus SurvivalStatus { get; }
        public bool IsPause { get; }
        public float SightR { get; }
        public SkillAction? SkillLaunch { get; }
        public bool SkillAct { get; }
        public bool IsStun { get; }
        public TwoDVector? IsBeHit { get; }
        public bool IsHitSome { get; }
        public ImmutableArray<TwoDPoint> SomeThings { get; }
        public ImmutableArray<int> KillList { get; }

        public CharTickMsg(int gid, TwoDPoint pos, TwoDVector aim, SurvivalStatus survivalStatus,
            SkillAction? skillLaunch,
            bool antiBuff, float speed, float sightR, bool isPause, bool skillAct, TwoDVector? isBeHit, bool isHitSome,
            ImmutableArray<TwoDPoint> someThings, ImmutableArray<int> killList)
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
            KillList = killList;
        }

        public override string ToString()
        {
            return
                $"gid: {Gid} Pos: {Pos} Aim {Aim} SightR {SightR} \n {SurvivalStatus}\n" +
                $" is on hit::{IsBeHit} , is stun :: {IsStun},skill act {SkillAct} launch {SkillLaunch} IsHitSth{IsHitSome}";
        }
    }
}