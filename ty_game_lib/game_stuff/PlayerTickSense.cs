using System;
using System.Collections.Immutable;
using System.Linq;

namespace game_stuff
{
    public readonly struct PlayerTickSense
    {
        public static PlayerTickSense Empty = new PlayerTickSense(ImmutableHashSet<ICanBeEnemy>.Empty,
            ImmutableHashSet<INotMoveCanBeAndNeedSew>.Empty, ImmutableHashSet<INotMoveCanBeAndNeedSew>.Empty,
            ImmutableHashSet<CharacterBody>.Empty, ImmutableHashSet<Bullet>.Empty,
            ImmutableHashSet<Bullet>.Empty);

        public ImmutableHashSet<ICanBeEnemy> OnChangingBodyAndRadarSee { get; }
        public ImmutableHashSet<CharacterBody> AppearBodies { get; }
        public ImmutableHashSet<INotMoveCanBeAndNeedSew> AppearNotMove { get; }
        public ImmutableHashSet<INotMoveCanBeAndNeedSew> VanishNotMove { get; }

        public ImmutableHashSet<Bullet> BulletSee { get; }

        public ImmutableHashSet<Bullet> BulletHear { get; }

        private PlayerTickSense(ImmutableHashSet<ICanBeEnemy> onChangingBodyAndRadarSee,
            ImmutableHashSet<INotMoveCanBeAndNeedSew> appearNotMove,
            ImmutableHashSet<INotMoveCanBeAndNeedSew> vanishNotMove, ImmutableHashSet<CharacterBody> appearBodies,
            ImmutableHashSet<Bullet> bulletSee, ImmutableHashSet<Bullet> bulletHear)
        {
            OnChangingBodyAndRadarSee = onChangingBodyAndRadarSee;
            AppearNotMove = appearNotMove;
            VanishNotMove = vanishNotMove;
            AppearBodies = appearBodies;
            BulletSee = bulletSee;
            BulletHear = bulletHear;
        }

        public static PlayerTickSense GenPlayerSense(
            (ImmutableHashSet<IPerceivable>, ImmutableHashSet<Bullet>, ImmutableHashSet<Bullet>) thisTickSee,
            (ImmutableHashSet<INotMoveCanBeAndNeedSew> lastTickSee, ImmutableHashSet<CharacterBody> characterBodies)
                lastTickSee)
        {
            var (thisTickPerceivable, bulletSee, hear) = thisTickSee;
            var characterBodies = thisTickPerceivable.OfType<CharacterBody>();
            var (lastNotMoveCanBeAndNeedSews, lastBodies) = lastTickSee;
            var newCharBodies = characterBodies.Except(lastBodies).ToImmutableHashSet();
            var onChange = thisTickPerceivable.OfType<ICanBeEnemy>().ToImmutableHashSet();
            var appear = thisTickPerceivable.Except(lastNotMoveCanBeAndNeedSews).OfType<INotMoveCanBeAndNeedSew>()
                .ToImmutableHashSet();
            var vanish = lastNotMoveCanBeAndNeedSews.Except(thisTickPerceivable).OfType<INotMoveCanBeAndNeedSew>()
                .ToImmutableHashSet();
#if DEBUG
            if (vanish.Any())
            {
                Console.Out.WriteLine($"vanish  {vanish.Count}");
            }

#endif
            var playerTickSee = new PlayerTickSense(onChange, appear, vanish, newCharBodies, bulletSee,
                hear);
            return playerTickSee;
        }
    }
}