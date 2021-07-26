using System;
using System.Linq;

namespace game_stuff
{
    public class StunFixStatus
    {
        public float MakeStunTickMulti { get; private set; }

        public float MakeStunForceMulti { get; private set; }

        public float TakeStunTickMulti { get; set; }

        public float TakeStunForceMulti { get; set; }

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
    }
}