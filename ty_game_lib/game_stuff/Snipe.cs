using System;
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


        public Snipe(snipe snipe, ImmutableDictionary<BodySize, float> snipeMulti,int maxStep)
        {
            if (snipe.TotalStep>maxStep)
            {
                throw new IndexOutOfRangeException($"not enough step for this snipe {snipe.id}");
            }
            TrickTick = snipe.TrickTick;
            MaxStep = snipe.TotalStep-1;
            MoveSpeedMulti = snipeMulti.ToDictionary(pair => pair.Key, pair => pair.Value * snipe.MoveMulti)
                .ToImmutableDictionary();
            AddStepPerTick = snipe.OnTickSpeed;
            OffStepPerTick = snipe.OffTickSpeed;
        }
    }
}