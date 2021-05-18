using System.Collections.Generic;
using System.Linq;

namespace collision_and_rigid
{
    public class AreaBox : IAaBbBox
    {
        private SimpleBlocks SimpleBlocks { get; }


        public AreaBox(Zone zone, SimpleBlocks simpleBlocks, int polyId)
        {
            Zone = zone;
            SimpleBlocks = simpleBlocks;
            PolyId = polyId;
        }

        public int PolyId { get; }


        public void WriteQuadRecord(Quad quad)
        {
        }

        public Quad? GetNextQuad()
        {
            return null;
        }

        public IShape GetShape()
        {
            return SimpleBlocks;
        }

        public TwoDPoint GetAnchor()
        {
            var (horizon, vertical) = Zone.GetMid();
            return new TwoDPoint(vertical, horizon);
        }

        public bool CanInteractive(TwoDPoint pos)
        {
            return false;
        }

        public Zone Zone { get; set; }

        public IEnumerable<(int, IAaBbBox)> SplitByQuads(Zone zone)
        {
            var (horizon, vertical) = zone.GetMid();
            return SplitByQuads(horizon, vertical);
        }

        public IEnumerable<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical)
        {
            return Zone.SplitByQuads(horizon, vertical)
                .Select(x => (x.Item1, new AreaBox(x.Item2, SimpleBlocks, PolyId) as IAaBbBox)).ToList();
            // var valueTuples = new List<(int, IAaBbBox)>();
            // if (Zone.Left >= vertical)
            // {
            //     if (Zone.Down >= horizon)
            //     {
            //         valueTuples.Add((1, this));
            //     }
            //
            //     else if (Zone.Up <= horizon)
            //     {
            //         valueTuples.Add((4, this));
            //     }
            //     else
            //     {
            //         var (up, down) = Zone.CutByH(horizon);
            //         valueTuples.Add((1, new AreaBox(up, SimpleBlocks, PolyId)));
            //         valueTuples.Add((4, new AreaBox(down, SimpleBlocks, PolyId)));
            //     }
            // }
            //
            // if (Zone.Right <= vertical)
            // {
            //     if (Zone.Down >= horizon)
            //     {
            //         valueTuples.Add((2, this));
            //     }
            //
            //     else if (Zone.Up <= horizon)
            //     {
            //         valueTuples.Add((3, this));
            //     }
            //     else
            //     {
            //         var (up, down) = Zone.CutByH(horizon);
            //         valueTuples.Add((2, new AreaBox(up, SimpleBlocks, PolyId)));
            //         valueTuples.Add((3, new AreaBox(down, SimpleBlocks, PolyId)));
            //     }
            // }
            // else
            // {
            //     if (Zone.Down >= horizon)
            //     {
            //         var (left, right) = Zone.CutByV(vertical);
            //
            //         valueTuples.Add((2, new AreaBox(left, SimpleBlocks, PolyId)));
            //         valueTuples.Add((1, new AreaBox(right, SimpleBlocks, PolyId)));
            //     }
            //
            //     else if (Zone.Up <= horizon)
            //     {
            //         var (left, right) = Zone.CutByV(vertical);
            //
            //         valueTuples.Add((3, new AreaBox(left, SimpleBlocks, PolyId)));
            //         valueTuples.Add((4, new AreaBox(right, SimpleBlocks, PolyId)));
            //     }
            //     else
            //     {
            //         valueTuples.Add((0, this));
            //     }
            // }
            // return valueTuples;
        }

        public bool IsInArea(TwoDPoint pt)
        {
            return Zone.IncludePt(pt) && SimpleBlocks.PtInShapeIncludeSide(pt);
        }
    }
}