using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public readonly struct PlayerTickSense
    {
        public ImmutableHashSet<IPerceivable> OnChange { get; }
        public ImmutableHashSet<CharacterBody> NewCharBodies { get; }
        public ImmutableHashSet<INotMoveCanBeAndNeedSew> Appear { get; }
        public ImmutableHashSet<INotMoveCanBeAndNeedSew> Vanish { get; }

        public ImmutableHashSet<Bullet> BulletSee { get; }

        public ImmutableHashSet<Bullet> BulletHear { get; }

        private PlayerTickSense(ImmutableHashSet<IPerceivable> onChange,
            ImmutableHashSet<INotMoveCanBeAndNeedSew> appear,
            ImmutableHashSet<INotMoveCanBeAndNeedSew> vanish, ImmutableHashSet<CharacterBody> newCharBodies,
            ImmutableHashSet<Bullet> bulletSee, ImmutableHashSet<Bullet> bulletHear)
        {
            OnChange = onChange;
            Appear = appear;
            Vanish = vanish;
            NewCharBodies = newCharBodies;
            BulletSee = bulletSee;
            BulletHear = bulletHear;
        }

        public static PlayerTickSense GenPlayerSense(
            (ImmutableHashSet<IPerceivable>, ImmutableHashSet<Bullet>, ImmutableHashSet<Bullet>) thisTickSee,
            (ImmutableHashSet<INotMoveCanBeAndNeedSew> lastTickSee, ImmutableHashSet<CharacterBody> characterBodies)
                lastTickSee)
        {
            var (perceivableS, hashSet, hear) = thisTickSee;
            var characterBodies = perceivableS.OfType<CharacterBody>();
            var (moveCanBeSews, immutableHashSet1) = lastTickSee;
            var enumerable = characterBodies.Except(immutableHashSet1).ToImmutableHashSet();
            var notMoveCanBeSews = perceivableS.OfType<INotMoveCanBeAndNeedSew>();
            var immutableHashSet = perceivableS.Except(notMoveCanBeSews);
            var appear = perceivableS.Except(moveCanBeSews).OfType<INotMoveCanBeAndNeedSew>().ToImmutableHashSet();
            var vanish = moveCanBeSews.Except(perceivableS).OfType<INotMoveCanBeAndNeedSew>().ToImmutableHashSet();
#if DEBUG
            if (vanish.Any())
            {
                Console.Out.WriteLine($"vanish  {vanish.Count}");
            }
         
#endif
            var playerTickSee = new PlayerTickSense(immutableHashSet, appear, vanish, enumerable, hashSet,
                hear);
            return playerTickSee;
        }
    }


    public readonly struct PlayGroundGoTickResult
    {
        public static PlayGroundGoTickResult Empty = new PlayGroundGoTickResult(
            ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>.Empty,
            ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>>.Empty,
            ImmutableDictionary<int, PlayerTickSense>.Empty,
            ImmutableDictionary<int, ImmutableArray<IToOutPutResult>>.Empty);


        public PlayGroundGoTickResult(ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>> characterGidBeHit,
            ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>> trapGidTidBeHit,
            ImmutableDictionary<int, PlayerTickSense> playerSee,
            ImmutableDictionary<int, ImmutableArray<IToOutPutResult>> actOutPut)
        {
            CharacterGidBeHit = characterGidBeHit;
            TrapGidTidBeHit = trapGidTidBeHit;
            PlayerSee = playerSee;
            ActOutPut = actOutPut;
            var hitSomeThing = characterGidBeHit.Values.SelectMany(x => x);
            var relationMsgS = trapGidTidBeHit.Values.SelectMany(x => x.Values.SelectMany(x => x));
            var characterHitSomeThing = hitSomeThing.Union(relationMsgS);
            var immutableDictionary = characterHitSomeThing.OfType<IHitMsg>().GroupBy(x => x.CasterOrOwner.GetId())
                .ToImmutableDictionary(x => x.Key, x => x.ToImmutableHashSet());
            CharacterHitSomeThing = immutableDictionary;
        }

        public ImmutableDictionary<int, ImmutableArray<IToOutPutResult>> ActOutPut { get; }
        public ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>> CharacterGidBeHit { get; }

        public ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>> TrapGidTidBeHit
        {
            get;
        }

        public ImmutableDictionary<int, PlayerTickSense> PlayerSee { get; }

        public ImmutableDictionary<int, ImmutableHashSet<IHitMsg>> CharacterHitSomeThing { get; }

        // public static  

        public void Deconstruct(out ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>> playerBeHit,
            out ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>> trapBeHit,
            out ImmutableDictionary<int, PlayerTickSense> playerSee,
            out ImmutableDictionary<int, ImmutableArray<IToOutPutResult>> playerTeleportTo,
            out ImmutableDictionary<int, ImmutableHashSet<IHitMsg>> hitSomething)
        {
            playerSee = PlayerSee;
            trapBeHit = TrapGidTidBeHit;
            playerBeHit = CharacterGidBeHit;
            playerTeleportTo = ActOutPut;
            hitSomething = CharacterHitSomeThing;
        }
    }
}