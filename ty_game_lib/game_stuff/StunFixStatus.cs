using System;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class StunFixStatus
    {
        public float MakeStunTickMulti { get; private set; }

        public float MakeStunForceMulti { get; private set; }

        public float TakeStunTickMulti { get; private set; }

        public float TakeStunForceMulti { get; private set; }

        public StunFixStatus()
        {
            TakeStunTickMulti = 1f;
            TakeStunForceMulti = 1f;
            MakeStunTickMulti = 1f;

            MakeStunForceMulti = 1f;
        }


        public static StunFixStatus Unit()
        {
            return new StunFixStatus();
        }

        public void PassiveEffectChange(float[] passiveTrait)
        {
            if (!passiveTrait.Any())
            {
                return;
            }

            MakeStunTickMulti = (1f + passiveTrait[1]) / (1f + passiveTrait[0]);
            MakeStunForceMulti = (1f + passiveTrait[3]) / (1f + passiveTrait[2]);

            TakeStunTickMulti = (1f + passiveTrait[4]) / (1f + passiveTrait[5]);
            TakeStunForceMulti = (1f + passiveTrait[6]) / (1f + passiveTrait[7]);
        }

        public string GetDetail()
        {
            return
                $"击退力比:{MathTools.Round(MakeStunForceMulti, 2)} 击晕时间比:{MathTools.Round(MakeStunTickMulti, 2)} 被击力比:{MathTools.Round(TakeStunForceMulti, 2)} 被击晕时间比:{MathTools.Round(TakeStunTickMulti, 2)}";
        }
    }
}