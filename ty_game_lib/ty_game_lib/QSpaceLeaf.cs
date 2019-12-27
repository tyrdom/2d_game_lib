using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ty_game_lib
{
    public class QSpaceLeaf : QSpace
    {
        public QSpaceLeaf(Quad? quad, Zone zone, List<AabbBoxShape> aabbPackPackBoxes)
        {
            TheQuad = quad;
            Zone = zone;
            AabbPackBoxes = aabbPackPackBoxes;
        }

        public sealed override Quad? TheQuad { get; set; }
        public sealed override Zone Zone { get; set; }

        public sealed override List<AabbBoxShape> AabbPackBoxes { get; set; }

        public override void Remove(AabbBoxShape boxShape)
        {
        }

        public override IEnumerable<AabbBoxShape> TouchBy(AabbBoxShape boxShape)
        {
            return boxShape.TryTouch(AabbPackBoxes);
        }

        public override void InsertBox(AabbBoxShape boxShape)
        {
            AabbPackBoxes.Add(boxShape);
        }

        public override QSpace TryCovToLimitQSpace(int limit)
        {
            if (AabbPackBoxes.Count <= limit)
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

        public override (int, AabbBoxShape?) touchWithARightShootPoint(TwoDPoint p)
        {
            return p.GenARightShootCrossAlotAabbBoxShape(Zone, AabbPackBoxes);
        }

        public override string outZones()
        {
            string s = "";
            foreach (var aabbBoxShape in AabbPackBoxes)
            {
                var zone = aabbBoxShape.Zone;
                s += SomeTools.ZoneLog(zone) + "\n";
            }

            return s;
        }

        public QSpace TryCovToBranch()
        {
            var one = new List<AabbBoxShape>();
            var two = new List<AabbBoxShape>();
            var three = new List<AabbBoxShape>();
            var four = new List<AabbBoxShape>();
            var zone = new List<AabbBoxShape>();
            var (item1, item2) = Zone.GetMid();
            Parallel.ForEach(AabbPackBoxes, aabbBoxShape =>
                {
                    var intTBoxShapes = aabbBoxShape.SplitByQuads(item1, item2);

                    foreach (var intTBoxShape in intTBoxShapes)
                    {
                        switch (intTBoxShape.Key)
                        {
                            case 0:
                                zone.Add(intTBoxShape.Value);
                                break;
                            case 1:
                                one.Add(intTBoxShape.Value);
                                break;
                            case 2:
                                two.Add(intTBoxShape.Value);
                                break;
                            case 3:
                                three.Add(intTBoxShape.Value);
                                break;
                            case 4:
                                four.Add(intTBoxShape.Value);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            );

            if (zone.Count == AabbPackBoxes.Count) return this;


            var zones = Zone.CutTo4(item1, item2);


            return new QSpaceBranch(TheQuad, Zone, zone, new QSpaceLeaf(Quad.One, zones[0], one),
                new QSpaceLeaf(Quad.Two, zones[1], two),
                new QSpaceLeaf(Quad.Three, zones[2], three), new QSpaceLeaf(Quad.Four, zones[3], four));
        }
    }
}