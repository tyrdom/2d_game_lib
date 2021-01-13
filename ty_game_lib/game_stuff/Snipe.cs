using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using game_config;

namespace game_stuff
{
    public class Snipe
    {
        public ImmutableDictionary<BodySize, float> MoveSpeedMulti { get; }

        public int MaxStep { get; }
        public int AddStepPerTick { get; }
        public int OffStepPerTick { get; }
        public int TrickTick { get; }


        public float? GetSpeedMulti(BodySize bodySize)
        {
            var value = MoveSpeedMulti.TryGetValue(bodySize, out var f) ? f : (float?) null;
            return value;
        }

      
        public Snipe(ImmutableDictionary<BodySize, float> moveSpeedMulti, int trickTick)
        {
            MoveSpeedMulti = moveSpeedMulti;
            TrickTick = trickTick;
        }

        public Snipe(snipe snipe, ImmutableDictionary<BodySize, float> snipeMulti)
        {
            TrickTick = snipe.TrickTick;
            MaxStep = snipe.TotalStep;
            MoveSpeedMulti = snipeMulti.ToDictionary(pair => pair.Key, pair => pair.Value * snipe.MoveMulti)
                .ToImmutableDictionary();
            AddStepPerTick = snipe.OnTickSpeed;
            OffStepPerTick = snipe.OffTickSpeed;
        }
    }
}