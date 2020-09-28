using System.Collections.Generic;

namespace collision_and_rigid
{
    public class IdPointBox : IAaBbBox
    {
        public IdPointBox(Zone zone, IIdPointShape idPointShape)
        {
            IdPointShape = idPointShape;
            Zone = zone;
        }

        public IIdPointShape IdPointShape { get; }

        public void WriteQuadRecord(Quad quad)
        {
        }

        public Quad? GetNextQuad()
        {
            return null;
        }

        public IShape GetShape()
        {
            return IdPointShape;
        }

        public bool CanInteractive(TwoDPoint pos)
        {
            return false;
        }

        public Zone Zone { get; set; }

        public List<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical)
        {
            var whichQ = IdPointShape.GetAnchor().WhichQ(horizon, vertical);
            return new List<(int, IAaBbBox)> {(whichQ, this)};
        }
    }
}