using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public readonly struct PlayerTickSee
    {
        public ImmutableHashSet<IPerceivable> OnChange { get; }
        public ImmutableHashSet<INotMoveCanBeSew> Appear { get; }
        public ImmutableHashSet<INotMoveCanBeSew> Vanish { get; }

        public PlayerTickSee(ImmutableHashSet<IPerceivable> onChange, ImmutableHashSet<INotMoveCanBeSew> appear,
            ImmutableHashSet<INotMoveCanBeSew> vanish) : this()
        {
            OnChange = onChange;
            Appear = appear;
            Vanish = vanish;
        }

        public static PlayerTickSee GenPlayerSee(ImmutableHashSet<IPerceivable> thisTickSee,
            ImmutableHashSet<INotMoveCanBeSew> lastTickSee)
        {
            var notMoveCanBeSews = thisTickSee.OfType<INotMoveCanBeSew>();
            var immutableHashSet = thisTickSee.Except(notMoveCanBeSews);
            var appear = thisTickSee.Except(lastTickSee).OfType<INotMoveCanBeSew>().ToImmutableHashSet();
            var vanish = lastTickSee.Except(thisTickSee).OfType<INotMoveCanBeSew>().ToImmutableHashSet();
            var playerTickSee = new PlayerTickSee(immutableHashSet, appear, vanish);
            return playerTickSee;
        }
    }


    public readonly struct PlayGroundGoTickResult
    {
        public PlayGroundGoTickResult(ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>> characterGidBeHit,
            ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>> trapGidTidBeHit,
            ImmutableDictionary<int, PlayerTickSee> playerSee,
            ImmutableDictionary<int, TelePortMsg> playerTeleportTo)
        {
            CharacterGidBeHit = characterGidBeHit;
            TrapGidTidBeHit = trapGidTidBeHit;
            PlayerSee = playerSee;
            PlayerTeleportTo = playerTeleportTo;
            var hitSomeThing = characterGidBeHit.Values.SelectMany(x => x);
            var relationMsgS = trapGidTidBeHit.Values.SelectMany(x => x.Values.SelectMany(x => x));
            var characterHitSomeThing = hitSomeThing.Union(relationMsgS);
            var immutableDictionary = characterHitSomeThing.OfType<IHitMsg>().GroupBy(x => x.CasterOrOwner.GetId())
                .ToImmutableDictionary(x => x.Key, x => x.ToImmutableHashSet());
            CharacterHitSomeThing = immutableDictionary;
        }

        public ImmutableDictionary<int, TelePortMsg> PlayerTeleportTo { get; }
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
            out ImmutableDictionary<int, TelePortMsg> playerTeleportTo,
            out ImmutableDictionary<int, ImmutableHashSet<IHitMsg>> hitSomething)
        {
            playerSee = PlayerSee;
            trapBeHit = TrapGidTidBeHit;
            playerBeHit = CharacterGidBeHit;
            playerTeleportTo = PlayerTeleportTo;
            hitSomething = CharacterHitSomeThing;
        }
    }
}