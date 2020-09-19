using System.Linq;
using System.Security.Cryptography;
using game_config;

namespace game_stuff
{
    public class Snipe
    {
        public float MoveSpeedMulti { get; }

        public float[] OnZoomStepList { get; }
        public int OnPerTick { get; }
        public int OffPerTick { get; }
        public Scope[] ZoomStepScopes { get; set; }

        public int TrickTick { get; }

        public Snipe(float moveSpeedMulti, float[] onZoomMulti, int trickTick)
        {
            MoveSpeedMulti = moveSpeedMulti;
            OnZoomStepList = onZoomMulti;

            TrickTick = trickTick;
        }

        public Snipe(snipe snipe, Scope defaultScope)
        {
            TrickTick = snipe.TrickTick;
            var snipeZoomMultiAdd = snipe.ZoomMulti - 1;
            OnZoomStepList = Enumerable.Range(1, snipe.TotalStep)
                .Select(x => 1 + x * snipeZoomMultiAdd / snipe.TotalStep)
                .ToArray();

            MoveSpeedMulti = snipe.MoveMulti;
            OnPerTick = snipe.OnTickSpeed;
            OffPerTick = snipe.OffTickSpeed;
            var scopes = OnZoomStepList.Select(defaultScope.GenNewScope).ToArray();
            ZoomStepScopes = scopes;
        }
    }
}