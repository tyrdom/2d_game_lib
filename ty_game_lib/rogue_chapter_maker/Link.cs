using System.Collections.Immutable;

namespace rogue_chapter_maker
{
    public enum Side
    {
        East,
        West,
        North,
        South
    }

    public class Link
    {
        public Link(Side side)
        {
            Side = side;
            LinkTo = null;
        }

        private Side Side { get; }

        private PointMap? LinkTo { get; set; }
    }
}