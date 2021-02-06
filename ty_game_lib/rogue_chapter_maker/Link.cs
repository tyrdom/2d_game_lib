using System;
using System.Collections.Immutable;
using game_config;

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
        public Link(direction side, PointMap inPointMap)
        {
            Side = side;
            InPointMap = inPointMap;
            LinkTo = null;
        }

        public PointMap InPointMap { get; }
        public direction Side { get; }

        public PointMap? LinkTo { get; private set; }

        public void SetLink(PointMap pointMap)
        {
            LinkTo = pointMap;
        }
    }
}