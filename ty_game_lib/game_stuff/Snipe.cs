using System.Linq;
using System.Security.Cryptography;
using game_config;

namespace game_stuff
{
    public class Snipe
    {
        public float MoveSpeedMulti { get; }

        public float[] OnZoomMultiList { get; }
        public float[] OffZoomMultiList { get; }

        public int TrickTick { get; }

        public Snipe(float moveSpeedMulti, float[] onZoomMulti, float[] offZoomMultiList, int trickTick)
        {
            MoveSpeedMulti = moveSpeedMulti;
            OnZoomMultiList = onZoomMulti;
            OffZoomMultiList = offZoomMultiList;
            TrickTick = trickTick;
        }

        public Snipe(snipe snipe)
        {
            TrickTick = snipe.TrickTick;
            var snipeZoomMultiAdd = snipe.ZoomMulti - 1;
            OnZoomMultiList = Enumerable.Range(1, snipe.OnTick)
                .Select(x => 1 + x * snipeZoomMultiAdd / snipe.OnTick)
                .ToArray();
            OffZoomMultiList = Enumerable.Range(1, snipe.OffTick - 1)
                .Select(x => snipe.ZoomMulti - x * snipeZoomMultiAdd / snipe.OffTick)
                .ToArray();
            MoveSpeedMulti = snipe.MoveMulti;
        }
    }
}