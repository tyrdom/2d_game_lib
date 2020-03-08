using System;
using System.Collections.Generic;
using System.Linq;

namespace collision_and_rigid
{
    public class QSpaceLeaf : QSpace
    {
        public QSpaceLeaf(Quad? quad, QSpaceBranch? father, Zone zone, HashSet<AabbBoxShape> aabbPackPackBoxShapes)
        {
            Father = father;
            TheQuad = quad;
            Zone = zone;
            AabbPackBoxShapes = aabbPackPackBoxShapes;
        }

        public sealed override Quad? TheQuad { get; set; }
        public sealed override Zone Zone { get; set; }

        public sealed override HashSet<AabbBoxShape> AabbPackBoxShapes { get; set; }

        public override void AddIdPoint(HashSet<AabbBoxShape> idPointShapes, int limit)
        {
            var rawCount = AabbPackBoxShapes.Count;
            if (rawCount + idPointShapes.Count <= limit)
            {
                AabbPackBoxShapes.UnionWith(idPointShapes);
            }
            else
            {
                var q1 = new HashSet<AabbBoxShape>();
                var q2 = new HashSet<AabbBoxShape>();
                var q3 = new HashSet<AabbBoxShape>();
                var q4 = new HashSet<AabbBoxShape>();
                foreach (var aabbBoxShape in idPointShapes)
                {
                    var shape = aabbBoxShape.Shape;
                    switch (shape)
                    {
                        case IIdPointShape idPointShape:
                            var twoDPoint = idPointShape.GetAnchor();
                            if (AabbPackBoxShapes.Count <= limit)
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
                QSpace qsl1 = new QSpaceLeaf(Quad.One, null, cutTo4[0], q1);
                QSpace qsl2 = new QSpaceLeaf(Quad.Two, null, cutTo4[1], q2);
                QSpace qsl3 = new QSpaceLeaf(Quad.Three, null, cutTo4[2], q3);
                QSpace qsl4 = new QSpaceLeaf(Quad.Four, null, cutTo4[3], q4);
                var qSpaceBranch = new QSpaceBranch(TheQuad, Father, Zone, AabbPackBoxShapes, ref qsl1, ref qsl2,
                    ref qsl3, ref qsl4);

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

        public override void MoveIdPoint(Dictionary<int, ITwoDTwoP> gidToMove, int limit)
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

        public override bool LineIsCross(TwoDVectorLine line)
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

        public override void ForeachDoWithOutMove<T>(Action<IIdPointShape, T> doWithIIdPointShape, T t)
        {
            foreach (var aabbPackBoxShape in AabbPackBoxShapes)
            {
                var shape = aabbPackBoxShape.Shape;
                switch (shape)
                {
                    case IIdPointShape idPointShape:
                        doWithIIdPointShape(idPointShape, t);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(shape));
                }
            }
        }


        public QSpace TryCovToBranch()
        {
            var one = new HashSet<AabbBoxShape>();
            var two = new HashSet<AabbBoxShape>();
            var three = new HashSet<AabbBoxShape>();
            var four = new HashSet<AabbBoxShape>();
            var zone = new HashSet<AabbBoxShape>();
            var (item1, item2) = Zone.GetMid();
            foreach (var aabbBoxShape in AabbPackBoxShapes)
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

            if (zone.Count == AabbPackBoxShapes.Count) return this;


            var zones = Zone.CutTo4(item1, item2);

            QSpace qs1 = new QSpaceLeaf(Quad.One, null, zones[0], one);
            QSpace qs2 = new QSpaceLeaf(Quad.Two, null, zones[1], two);
            QSpace qs3 = new QSpaceLeaf(Quad.Three, null, zones[2], three);
            QSpace qs4 = new QSpaceLeaf(Quad.Four, null, zones[3], four);
            var tryCovToBranch = new QSpaceBranch(TheQuad, Father, Zone, zone,
                ref qs1,
                ref qs2,
                ref qs3,
                ref qs4);
            tryCovToBranch.QuadOne.Father = tryCovToBranch;
            tryCovToBranch.QuadTwo.Father = tryCovToBranch;
            tryCovToBranch.QuadThree.Father = tryCovToBranch;
            tryCovToBranch.QuadFour.Father = tryCovToBranch;
            return tryCovToBranch;
        }
    }
}