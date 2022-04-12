using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public readonly struct StepBuffs
    {
        public int Step { get; }

        public PlayingBuffsMaker PlayingBuffs { get; }

        public StepBuffs(int step, PlayingBuffsMaker playingBuffs)
        {
            Step = step;
            PlayingBuffs = playingBuffs;
        }
    }

    public class Snipe
    {
        public ImmutableDictionary<size, float> MoveSpeedMulti { get; }

        public int MaxStep { get; }
        public int AddStepPerTick { get; }
        public int OffStepPerTick { get; }
        public int TrickTick { get; }

        public StepBuffs[] StepToAddBuff { get; }

        public float? GetSpeedMulti(size bodySize)
        {
            var value = MoveSpeedMulti.TryGetValue(bodySize, out var f) ? f : (float?)null;
            return value;
        }


        public Snipe(snipe snipe, ImmutableDictionary<size, float> snipeMulti, int maxStep)
        {
            if (snipe.TotalStep > maxStep)
            {
                throw new IndexOutOfRangeException($"not enough step for this snipe {snipe.id}");
            }

            TrickTick = snipe.TrickTick;

            MaxStep = MathTools.Min(maxStep, snipe.TotalStep) - 1;
            MoveSpeedMulti = snipeMulti.ToDictionary(pair => pair.Key, pair => pair.Value * snipe.MoveMulti)
                .ToImmutableDictionary();
            AddStepPerTick = snipe.OnTickSpeed;
            OffStepPerTick = snipe.OffTickSpeed;

            if (snipe.StepToBuff.Any())
            {
                var list = snipe.StepToBuff.Select(x =>
                    new StepBuffs(x.Key - 1, new PlayingBuffsMaker(x.Value))).ToList();

                list.Sort((x, y) => x.Step.CompareTo(y.Step));

                var stepBuffsArray = list.ToArray();
                StepToAddBuff = stepBuffsArray;
            }
            else
            {
                StepToAddBuff = Array.Empty<StepBuffs>();
            }
        }
    }
}