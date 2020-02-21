using System;
using System.Collections.Generic;

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

        public sealed override Quad? TheQuad { get; set; }
        public sealed override Zone Zone { get; set; }

        public sealed override List<AabbBoxShape> AabbPackBoxShapes { get; set; }

        public override void AddIdPoint(List<AabbBoxShape> idPointShapes, int limit)
        {
            var rawCount = AabbPackBoxShapes.Count;
            if (rawCount + idPointShapes.Count <= limit)
            {
                AabbPackBoxShapes.AddRange(idPointShapes);
            }
            else
            {
                var q1 = new List<AabbBoxShape>();
                var q2 = new List<AabbBoxShape>();
                var q3 = new List<AabbBoxShape>();
                var q4 = new List<AabbBoxShape>();
                for (var index = 0; index < idPointShapes.Count; index++)
                {
                    var aabbBoxShape = idPointShapes[index];
                    var shape = aabbBoxShape.Shape;
                    switch (shape)
                    {
                        case IIdPointShape idPointShape:
                            var twoDPoint = idPointShape.GetAchor();
                            if (index + rawCount < limit)
                            {
                                AabbPackBoxShapes.Add(aabbBoxShape);
                            }
                            else
                            {
                                var whichQ = twoDPoint.WhichQ(Zone);
                                switch (whichQ)
                                {
                                    case Quad.One:
                                        q1.Add(aabbBoxShape);
                                        break;
                                    case Quad.Two:
                                        q2.Add(aabbBoxShape);
                                        break;
                                    case Quad.Three:
                                        q3.Add(aabbBoxShape);
                                        break;
                                    case Quad.Four:
                                        q4.Add(aabbBoxShape);
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(shape));
                    }
                }

                var (item1, item2) = Zone.GetMid();
                var cutTo4 = Zone.CutTo4(item1, item2);
                var qsl1 = new QSpaceLeaf(Quad.One, null, cutTo4[0], q1);
                var qsl2 = new QSpaceLeaf(Quad.One, null, cutTo4[1], q2);
                var qsl3 = new QSpaceLeaf(Quad.One, null, cutTo4[2], q3);
                var qsl4 = new QSpaceLeaf(Quad.One, null, cutTo4[3], q4);
                var qSpaceBranch = new QSpaceBranch(TheQuad, Father, Zone, AabbPackBoxShapes, qsl1, qsl2, qsl3, qsl4);
                qSpaceBranch.QuadOne.Father = qSpaceBranch;
                qSpaceBranch.QuadTwo.Father = qSpaceBranch;
                qSpaceBranch.QuadThree.Father = qSpaceBranch;
                qSpaceBranch.QuadFour.Father = qSpaceBranch;
                if (Father != null)
                {
                    switch (TheQuad)
                    {
                        case Quad.One:
                            Father.QuadOne = qSpaceBranch;
                            break;
                        case Quad.Two:
                            Father.QuadTwo = qSpaceBranch;
                            break;
                        case Quad.Three:
                            Father.QuadThree = qSpaceBranch;
                            break;
                        case Quad.Four:
                            Father.QuadFour = qSpaceBranch;
                            break;
                        case null:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public override void MoveIdPoint(Dictionary<int, TwoDVector> gidToMove, int limit)
        {
            var (inZone, outZone) = SomeTools.MovePtsReturnInAndOut(gidToMove, AabbPackBoxShapes, Zone);
            AabbPackBoxShapes = inZone;

            Father.AddIdPoint(outZone, limit);
        }

        public override TwoDPoint GetSlidePoint(TwoDVectorLine line, bool isPush, bool safe = true)
        {
            var notCross = Zone.NotCross(line.GenZone());
            return notCross ? null : SomeTools.SlideTwoDPoint(AabbPackBoxShapes, line, isPush, safe);
        }

        public override void Remove(AabbBoxShape boxShape)
        {
            AabbPackBoxShapes.Remove(boxShape);
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

            var tryCovToBranch = TryCovToBranch();
            return tryCovToBranch switch
            {
                QSpaceBranch qSpaceBranch => qSpaceBranch.TryCovToLimitQSpace(limit),
                QSpaceLeaf _ => this,
                _ => throw new ArgumentOutOfRangeException(nameof(tryCovToBranch))
            };
        }

        public override (int, AabbBoxShape?) TouchWithARightShootPoint(TwoDPoint p)
        {
            return p.GenARightShootCrossALotAabbBoxShape(AabbPackBoxShapes);
        }

        public override string OutZones()
        {
            var s = "Leaf:::" + AabbPackBoxShapes.Count + "\n";
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

        public override void ForeachDoWithOutMove<T>(Action<IIdPointShape, T> doWithIIdPointShape,T t)
        {
            foreach (var aabbPackBoxShape in AabbPackBoxShapes)
            {
                var shape = aabbPackBoxShape.Shape;
                switch (shape)
                {
                    case IIdPointShape idPointShape:
                        doWithIIdPointShape(idPointShape,t);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(shape));
                }
            }
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
            );

            if (zone.Count == AabbPackBoxShapes.Count) return this;


            var zones = Zone.CutTo4(item1, item2);


            var tryCovToBranch = new QSpaceBranch(TheQuad, Father, Zone, zone,
                new QSpaceLeaf(Quad.One, null, zones[0], one),
                new QSpaceLeaf(Quad.Two, null, zones[1], two),
                new QSpaceLeaf(Quad.Three, null, zones[2], three),
                new QSpaceLeaf(Quad.Four, null, zones[3], four));
            tryCovToBranch.QuadOne.Father = tryCovToBranch;
            tryCovToBranch.QuadTwo.Father = tryCovToBranch;
            tryCovToBranch.QuadThree.Father = tryCovToBranch;
            tryCovToBranch.QuadFour.Father = tryCovToBranch;
            return tryCovToBranch;
        }
    }
}