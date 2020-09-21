using System.Linq;
using System.Security.Cryptography;
using game_config;

namespace game_stuff
{
    public class Snipe
    {
        public float MoveSpeedMulti { get; }

        public int MaxStep { get; }
        public int AddStepPerTick { get; }
        public int OffStepPerTick { get; }
        public int TrickTick { get; }

        public Snipe(float moveSpeedMulti, float[] onZoomMulti, int trickTick)
        {
            MoveSpeedMulti = moveSpeedMulti;


            TrickTick = trickTick;
        }

        public Snipe(snipe snipe)
        {
            TrickTick = snipe.TrickTick;
            MaxStep = snipe.TotalStep;
            MoveSpeedMulti = snipe.MoveMulti;
            AddStepPerTick = snipe.OnTickSpeed;
            OffStepPerTick = snipe.OffTickSpeed;
        }
    }
}