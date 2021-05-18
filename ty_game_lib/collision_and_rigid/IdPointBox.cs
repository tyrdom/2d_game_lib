using System.Collections.Generic;
using System.Linq;

namespace collision_and_rigid
{
    public class IdPointBox : IAaBbBox, IRecordQuadBox
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

        public IEnumerable<(int, IAaBbBox)> SplitByQuads(Zone zone)
        {
            var (horizon, vertical) = zone.GetMid();
            var twoDPoint = IdPointShape.GetAnchor();
            var whichQ = zone.IncludePt(twoDPoint) ? twoDPoint.WhichQ(horizon, vertical) : -1;
            return new List<(int, IAaBbBox)> {(whichQ, this)};
        }
    }

    public interface IRecordQuadBox
    {
    }
}