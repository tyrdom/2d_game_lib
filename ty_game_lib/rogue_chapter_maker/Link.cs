using System;
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
        public Link(Side side, PointMap inPointMap)
        {
            Side = side;
            InPointMap = inPointMap;
            LinkTo = null;
        }

        public PointMap InPointMap { get; }
        public Side Side { get; }

        public PointMap? LinkTo { get; private set; }

        public void SetLink(PointMap pointMap)
        {
            LinkTo = pointMap;
        }
    }
}