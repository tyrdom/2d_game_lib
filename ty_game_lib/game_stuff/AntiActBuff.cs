using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public abstract class AntiActBuff
    {
        public abstract int RestTick { get; set; }

        public abstract (TwoDPoint, AntiActBuff?) GoATick(TwoDPoint oldPt);
    }

    class PushOnEarth : AntiActBuff
    {
        public TwoDVector PushVector;
        public TwoDVector DecreasePerTick;

        public PushOnEarth(TwoDVector pushVector, TwoDVector decreasePerTick, int restTick)
        {
            PushVector = pushVector;
            DecreasePerTick = decreasePerTick;
            RestTick = restTick;
        }

        public sealed override int RestTick { get; set; }

        public override (TwoDPoint, AntiActBuff?) GoATick(TwoDPoint oldPt)
        {
            RestTick -= 1;
            if (RestTick <= 0)
            {
                return (oldPt.Move(PushVector), null);
            }

            var push = PushVector;
            var twoDVector = push.Minus(DecreasePerTick);
            var dot = push.Dot(twoDVector);
            if (dot > 0)
            {
                twoDVector = new TwoDVector(0f, 0f);
            }

            PushVector = twoDVector;

            return (oldPt.Move(push), this);
        }
    }

    public class PushOnAir : AntiActBuff
    {
        public TwoDVector PushVector;
        public float Height;
        public float UpSpeed;


        public PushOnAir(TwoDVector pushVector, float height, float upSpeed, int restTick)
        {
            PushVector = pushVector;
            Height = height;
            UpSpeed = upSpeed;
            RestTick = restTick;
        }

        public sealed override int RestTick { get; set; }

        public override (TwoDPoint, AntiActBuff?) GoATick(TwoDPoint oldPt)
        {
            var nowHeight = Height + UpSpeed;
            RestTick -= 1;
            if (RestTick <= 0)
            {
                return (oldPt.Move(PushVector), null);
            }

            if (nowHeight <= 0)
            {
                var pushOnEarth =
                    new PushOnEarth(PushVector, PushVector.GetUnit().Multi(TempConfig.Friction), RestTick);
                return (oldPt.Move(PushVector), pushOnEarth);
            }

            var nowUpSpeed = UpSpeed - TempConfig.G;
            Height = nowHeight;
            UpSpeed = nowUpSpeed;
            return (oldPt.Move(PushVector), this);
        }
    }

    class Caught : AntiActBuff
    {
        

        public List<TwoDPoint> MovesOnPoints;
        public CharacterStatus WhoCatchMe;
        public Caught( List<TwoDPoint> movesOnPoints, int restTick,ref CharacterStatus whoCatchMe)
        {
            
            MovesOnPoints = movesOnPoints;
            RestTick = restTick;
            WhoCatchMe = whoCatchMe;
        }

        public sealed override int RestTick { get; set; }

        public override (TwoDPoint, AntiActBuff?) GoATick(TwoDPoint oldPt)
        {
            RestTick -= 1;
            var count = MovesOnPoints.Count;

            var pt = count <= 0 ? oldPt : MovesOnPoints[0];

            if (RestTick <= 0)
            {
                return (pt, null);
            }

            if (count > 0)
            {
                MovesOnPoints.RemoveAt(0);
            }

            return (pt, this);
        }
    }
}