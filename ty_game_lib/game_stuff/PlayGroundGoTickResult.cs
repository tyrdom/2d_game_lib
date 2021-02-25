using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace game_stuff
{
    public readonly struct PlayGroundGoTickResult
    {
        public PlayGroundGoTickResult(ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>> characterBeHit,
            ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>> trapBeHit,
            ImmutableDictionary<int, ImmutableHashSet<IPerceivable>> playerSee,
            ImmutableDictionary<int, TelePortMsg> playerTeleportTo)
        {
            CharacterBeHit = characterBeHit;
            TrapBeHit = trapBeHit;
            PlayerSee = playerSee;
            PlayerTeleportTo = playerTeleportTo;
        }

        public ImmutableDictionary<int, TelePortMsg> PlayerTeleportTo { get; }
        public ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>> CharacterBeHit { get; }
        public ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>> TrapBeHit { get; }
        public ImmutableDictionary<int, ImmutableHashSet<IPerceivable>> PlayerSee { get; }


        public static PlayGroundGoTickResult Sum(IEnumerable<PlayGroundGoTickResult> playGroundGoTickResults)
        {
            var hit = new Dictionary<int, ImmutableHashSet<IRelationMsg>>();
            var trap = new Dictionary<int, ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>>();
            var see = new Dictionary<int, ImmutableHashSet<IPerceivable>>();
            var ints = new Dictionary<int, TelePortMsg>();
            var (hit1, trap1, see2, dic) =
                playGroundGoTickResults.Aggregate((hit, trap, see, ints), (s, x) =>
                {
                    var (dictionary, dictionary1, see1, ins) = s;
                    var keyValuePairs = dictionary1.Union(x.TrapBeHit);
                    var valuePairs = dictionary.Union(x.CharacterBeHit);
                    var enumerable = see1.Union(x.PlayerSee);
                    var union = ins.Union(x.PlayerTeleportTo);
                    return ((Dictionary<int, ImmutableHashSet<IRelationMsg>> hit,
                        Dictionary<int, ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>> trap,
                        Dictionary<int, ImmutableHashSet<IPerceivable>> see, Dictionary<int, TelePortMsg> ints)) (
                        valuePairs,
                        keyValuePairs, enumerable, union);
                });
            return new PlayGroundGoTickResult(hit1.ToImmutableDictionary(), trap1.ToImmutableDictionary(),
                see2.ToImmutableDictionary(), dic.ToImmutableDictionary());
        }

        public void Deconstruct(out ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>> playerBeHit,
            out ImmutableDictionary<int, ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>>> trapBeHit,
            out ImmutableDictionary<int, ImmutableHashSet<IPerceivable>> playerSee,
            out ImmutableDictionary<int, TelePortMsg> playerTeleportTo)
        {
            playerSee = PlayerSee;
            trapBeHit = TrapBeHit;
            playerBeHit = CharacterBeHit;
            playerTeleportTo = PlayerTeleportTo;
        }
    }
}