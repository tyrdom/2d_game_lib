using System.Collections.Generic;
using System.Linq;

namespace collision_and_rigid
{
    public class IdPointBox : IAaBbBox
    {
        public IdPointBox(Zone zone, IIdPointShape idPointShape)
        {
            IdPointShape = idPointShape;
            Zone = zone;

            LocateRecord = new List<Quad>();
            idPointShape.IdPointBox = this;
        }

        public bool RemoveLastRecord()
        {
            return LocateRecord.Remove(LocateRecord.LastOrDefault());
        }

        public void AddRecord(Quad quad)
        {
            LocateRecord.Add(quad);
        }

        public Quad? ReadRecord()
        {
            if (LocateRecord.Count <= 0) return null;
            var firstOrDefault = LocateRecord[0];
            LocateRecord.Remove(firstOrDefault);
            return firstOrDefault;
        }

        public List<Quad> LocateRecord { get; }
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

        public int GetId()
        {
            return IdPointShape.GetId();
        }

        public TwoDPoint GetAnchor()
        {
            return IdPointShape.GetAnchor();
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