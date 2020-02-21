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

        public CharacterBody(TwoDPoint nowPos, BodySize bodySize, CharacterStatus characterStatus, TwoDPoint lastPos,
            AngleSight sight)
        {
            NowPos = nowPos;
            BodySize = bodySize;
            CharacterStatus = characterStatus;
            LastPos = lastPos;
            Sight = sight;
        }

        public AabbBoxShape CovToAabbPackBox()
        {
            var zone = new Zone(0f,0f,0f,0f);
            return new AabbBoxShape(zone, this);
        }


        public int TouchByRightShootPointInAAbbBox(TwoDPoint p)
        {
            throw new NotImplementedException();
        }

        public bool IsTouchAnother(IShape another)
        {
            throw new NotImplementedException();
        }

        public int GetId()
        {
            return CharacterStatus.GId;
        }

        public TwoDPoint Move(TwoDVector vector)
        {
            LastPos = NowPos;
            var twoDPoint = NowPos.Move(vector);
            NowPos = twoDPoint;
            return NowPos;
        }

        public TwoDPoint GetAchor()
        {
            return NowPos;
        }

        public void DoOpFromDic(Dictionary<int,Operate> gidToOp)
        {
            throw new NotImplementedException();
        }
    }
}