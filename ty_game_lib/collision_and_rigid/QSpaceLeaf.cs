using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace collision_and_rigid
{
    public class QSpaceLeaf : QSpace
    {
        public QSpaceLeaf(Quad? quad, QSpaceBranch? father, Zone zone, List<AabbBoxShape> aabbPackPackBoxShapes)
        {
            Father = father;
            TheQuad = quad;
            Zone = zone;
            AabbPackBoxShapes = aabbPackPackBoxShapes;
        }

        public override TwoDPoint GetSlidePoint(TwoDVectorLine line, bool isPush, bool safe = true)
        {
            var notCross = Zone.NotCross(line.GenZone());
            return notCross ? null : SomeTools.SlideTwoDPoint(AabbPackBoxShapes, line, isPush, safe);
        }

        public sealed override Quad? TheQuad { get; set; }
        public sealed override Zone Zone { get; set; }

        public sealed override List<AabbBoxShape> AabbPackBoxShapes { get; set; }

        public override void Remove(AabbBoxShape boxShape)
        {
        }

        public override IEnumerable<AabbBoxShape> TouchBy(AabbBoxShape boxShape)
        {
            return boxShape.TryTouch(AabbPackBoxShapes);
        }

        public override bool IsTouchBy(AabbBoxShape boxShape)
        {
            throw new NotImplementedException();
        }

        public override void InsertBox(AabbBoxShape boxShape)
        {
            AabbPackBoxShapes.Add(boxShape);
        }

        public override QSpace TryCovToLimitQSpace(int limit)
        {
            if (AabbPackBoxShapes.Count <= limit)
            {
                return this;
            }
            else
            {
                var tryCovToBranch = TryCovToBranch();
                return tryCovToBranch switch
                {
                    QSpaceBranch qSpaceBranch => qSpaceBranch.TryCovToLimitQSpace(limit),
                    QSpaceLeaf _ => this,
                    _ => throw new ArgumentOutOfRangeException(nameof(tryCovToBranch))
                };
            }
        }

        public override (int, AabbBoxShape?) TouchWithARightShootPoint(TwoDPoint p)
        {
            return p.GenARightShootCrossALotAabbBoxShape(AabbPackBoxShapes);
        }

        public override string OutZones()
        {
            string s = "Leaf:::" + AabbPackBoxShapes.Count + "\n";
            foreach (var aabbBoxShape in AabbPackBoxShapes)
            {
                var zone = aabbBoxShape.Zone;
                s += SomeTools.ZoneLog(zone) + "\n";
            }

            return s;
        }

        public override int FastTouchWithARightShootPoint(TwoDPoint p)
        {
            var i = p.FastGenARightShootCrossALotAabbBoxShape(AabbPackBoxShapes);
            return i;
        }

        public QSpace TryCovToBranch()
        {
            var one = new List<AabbBoxShape>();
            var two = new List<AabbBoxShape>();
            var three = new List<AabbBoxShape>();
            var four = new List<AabbBoxShape>();
            var zone = new List<AabbBoxShape>();
            var (item1, item2) = Zone.GetMid();
            AabbPackBoxShapes.ForEach(aabbBoxShape =>
                {
                    var intTBoxShapes = aabbBoxShape.SplitByQuads(item1, item2);

                    foreach (var (i, aabbBoxShape1) in intTBoxShapes)
                    {
                        switch (i)
                        {
                            case 0:
                                zone.Add(aabbBoxShape1);
                                break;
                            case 1:
                                one.Add(aabbBoxShape1);
                                break;
                            case 2:
                                two.Add(aabbBoxShape1);
                                break;
                            case 3:
                                three.Add(aabbBoxShape1);
                                break;
                            case 4:
                                four.Add(aabbBoxShape1);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            );

            if (zone.Count == AabbPackBoxShapes.Count) return this;


            var zones = Zone.CutTo4(item1, item2);


            var tryCovToBranch = new QSpaceBranch(TheQuad, Father, Zone, zone,
                new QSpaceLeaf(Quad.One, null, zones[0], one),
                new QSpaceLeaf(Quad.Two, null, zones[1], two),
                new QSpaceLeaf(Quad.Three, null, zones[2], three),
                new QSpaceLeaf(Quad.Four, null, zones[3], four));

            return tryCovToBranch;
        }
    }
}