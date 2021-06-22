using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public readonly struct PlayerTickSee
    {
        public ImmutableHashSet<IPerceivable> OnChange { get; }
        public ImmutableHashSet<CharacterBody> NewCharBodies { get; }
        public ImmutableHashSet<INotMoveCanBeAndNeedSew> Appear { get; }
        public ImmutableHashSet<INotMoveCanBeAndNeedSew> Vanish { get; }

        private PlayerTickSee(ImmutableHashSet<IPerceivable> onChange, ImmutableHashSet<INotMoveCanBeAndNeedSew> appear,
            ImmutableHashSet<INotMoveCanBeAndNeedSew> vanish, ImmutableHashSet<CharacterBody> newCharBodies)
        {
            OnChange = onChange;
            Appear = appear;
            Vanish = vanish;
            NewCharBodies = newCharBodies;
        }

        public static PlayerTickSee GenPlayerSee(ImmutableHashSet<IPerceivable> thisTickSee,
            (ImmutableHashSet<INotMoveCanBeAndNeedSew> lastTickSee, ImmutableHashSet<CharacterBody> characterBodies)
                lastTickSee)
        {
            var characterBodies = thisTickSee.OfType<CharacterBody>();
            var (moveCanBeSews, immutableHashSet1) = lastTickSee;
            var enumerable = characterBodies.Except(immutableHashSet1).ToImmutableHashSet();
            var notMoveCanBeSews = thisTickSee.OfType<INotMoveCanBeAndNeedSew>();
            var immutableHashSet = thisTickSee.Except(notMoveCanBeSews);
            var appear = thisTickSee.Except(moveCanBeSews).OfType<INotMoveCanBeAndNeedSew>().ToImmutableHashSet();
            var vanish = moveCanBeSews.Except(thisTickSee).OfType<INotMoveCanBeAndNeedSew>().ToImmutableHashSet();
            var playerTickSee = new PlayerTickSee(immutableHashSet, appear, vanish, enumerable);
            return playerTickSee;
        }
    }


    public readonly struct PlayGroundGoTickResult
    {
        public static PlayGroundGoTickResult Empty = new PlayGroundGoTickResult(
            ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>.Empty,
            ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>>.Empty,
            ImmutableDictionary<int, PlayerTickSee>.Empty,
            ImmutableDictionary<int, ImmutableArray<IToOutPutResult>>.Empty);


        public PlayGroundGoTickResult(ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>> characterGidBeHit,
            ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>> trapGidTidBeHit,
            ImmutableDictionary<int, PlayerTickSee> playerSee,
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

        public ImmutableDictionary<int, PlayerTickSee> PlayerSee { get; }

        public ImmutableDictionary<int, ImmutableHashSet<IHitMsg>> CharacterHitSomeThing { get; }

        // public static  

        public void Deconstruct(out ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>> playerBeHit,
            out ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>> trapBeHit,
            out ImmutableDictionary<int, PlayerTickSee> playerSee,
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