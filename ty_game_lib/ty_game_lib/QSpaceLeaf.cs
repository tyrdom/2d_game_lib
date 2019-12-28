using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ty_game_lib
{
    public class QSpaceLeaf : QSpace
    {
        public QSpaceLeaf(Quad? quad, Zone zone, List<AabbBoxShape> aabbPackPackBoxShapes)
        {
            TheQuad = quad;
            Zone = zone;
            AabbPackBoxShapes = aabbPackPackBoxShapes;
        }

        public override TwoDPoint? GetSlidePoint(AabbBoxShape lineInBoxShape)
        {
            return SomeTools.SlideTwoDPoint(AabbPackBoxShapes,lineInBoxShape);
            foreach (var aabbBoxShape in AabbPackBoxShapes)
            {
                var notCross = lineInBoxShape.Zone.NotCross(aabbBoxShape.Zone);
                if (!notCross)
                {
                    switch (lineInBoxShape._shape)
                    {
                        case TwoDVectorLine moveLine:
                            switch (aabbBoxShape._shape)
                            {
                                case ClockwiseTurning blockClockwiseTurning:
                                    var isCross = blockClockwiseTurning.IsCross(moveLine);
                                    if (isCross)
                                    {
                                        var twoDPoint = blockClockwiseTurning.Slide(moveLine.B);
                                        return twoDPoint;
                                    }

                                    break;
                                case TwoDVectorLine blockLine:
                                    var isCrossAnother = blockLine.SimpleIsCross(moveLine);
                                    if (isCrossAnother)
                                    {
                                        var twoDPoint = blockLine.Slide(moveLine.B);
                                        return twoDPoint;
                                    }

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return null;
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
            return p.GenARightShootCrossAlotAabbBoxShape(Zone, AabbPackBoxShapes);
        }

        public override string OutZones()
        {
            string s = "";
            foreach (var aabbBoxShape in AabbPackBoxShapes)
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
            Parallel.ForEach(AabbPackBoxShapes, aabbBoxShape =>
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

            if (zone.Count == AabbPackBoxShapes.Count) return this;


            var zones = Zone.CutTo4(item1, item2);


            return new QSpaceBranch(TheQuad, Zone, zone, new QSpaceLeaf(Quad.One, zones[0], one),
                new QSpaceLeaf(Quad.Two, zones[1], two),
                new QSpaceLeaf(Quad.Three, zones[2], three), new QSpaceLeaf(Quad.Four, zones[3], four));
        }
    }
}