using System.Collections.Concurrent;
using System.Collections.Generic;

namespace collision_and_rigid
{
    public interface IAaBbBox : IHaveAnchor
    {
        public Quad? GetNextQuad();
        public IShape GetShape();
        public bool CanInteractive(TwoDPoint pos);
        public Zone Zone { get; set; }
        public IEnumerable<(int, IAaBbBox)> SplitByQuads(Zone zone);
    }
}