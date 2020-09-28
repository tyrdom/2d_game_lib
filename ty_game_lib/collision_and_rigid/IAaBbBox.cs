using System.Collections.Concurrent;
using System.Collections.Generic;

namespace collision_and_rigid
{
    public interface IAaBbBox
    {
        public void WriteQuadRecord(Quad quad);
        public Quad? GetNextQuad();
        public IShape GetShape();

        public bool CanInteractive(TwoDPoint pos);
        public Zone Zone { get; set; }
        public List<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical);
    }
}