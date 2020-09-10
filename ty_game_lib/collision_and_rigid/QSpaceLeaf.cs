using System;
using System.Collections.Generic;
using System.Linq;

namespace collision_and_rigid
{
    public class QSpaceLeaf : IQSpace
    {
        public QSpaceLeaf(Quad? quad, QSpaceBranch? father, Zone zone, HashSet<IAaBbBox> aaBbPackPackBox)
        {
            Father = father;
            TheQuad = quad;
            Zone = zone;
            AaBbPackBox = aaBbPackPackBox;
        }

        public int Count()
        {
            var count = AaBbPackBox.Count;
            return count;
        }

        public Quad? TheQuad { get; set; }
        public QSpaceBranch? Father { get; set; }
        public Zone Zone { get; set; }

        public HashSet<IAaBbBox> AaBbPackBox { get; set; }

        public void AddIdPoint(HashSet<IdPointBox> idPointShapes, int limit)
        {
            var rawCount = AaBbPackBox.Count;
            if (rawCount + idPointShapes.Count <= limit)
            {
                AaBbPackBox.UnionWith(idPointShapes);
            }
            else
            {
                var q1 = new HashSet<IAaBbBox>();
                var q2 = new HashSet<IAaBbBox>();
                var q3 = new HashSet<IAaBbBox>();
                var q4 = new HashSet<IAaBbBox>();
                foreach (var aabbBoxShape in idPointShapes)
                {
                    var shape = aabbBoxShape.GetShape();
                    switch (shape)
                    {
                        case IIdPointShape idPointShape:
                            var twoDPoint = idPointShape.GetAnchor();
                            if (AaBbPackBox.Count <= limit)
                            {
                                AaBbPackBox.Add(aabbBoxShape);
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


                var cutTo4 = Zone.CutTo4();
                IQSpace qsl1 = new QSpaceLeaf(Quad.One, null, cutTo4[0], q1);
                IQSpace qsl2 = new QSpaceLeaf(Quad.Two, null, cutTo4[1], q2);
                IQSpace qsl3 = new QSpaceLeaf(Quad.Three, null, cutTo4[2], q3);
                IQSpace qsl4 = new QSpaceLeaf(Quad.Four, null, cutTo4[3], q4);
                var qSpaceBranch = new QSpaceBranch(TheQuad, Father, Zone, AaBbPackBox, qsl1, qsl2,
                    qsl3, qsl4);

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

        public void MoveIdPoint(Dictionary<int, ITwoDTwoP> gidToMove, int limit)
        {
            var (inZone, outZone) =
                SomeTools.MovePtsReturnInAndOut(gidToMove, AaBbPackBox.OfType<IdPointBox>(), Zone);

            AaBbPackBox = inZone;
            if (Father != null)
            {
                Father.AddIdPoint(outZone, limit);
            }
            else
            {
                AaBbPackBox.UnionWith(outZone);
            }
        }

        public void RemoveIdPoint(IdPointBox idPointBox)
        {
            AaBbPackBox.Remove(idPointBox);
        }

        public TwoDPoint? GetSlidePoint(TwoDVectorLine line, bool safe = true)
        {
            var notCross = Zone.RealNotCross(line.GenZone());


            TwoDPoint? slideTwoDPoint;
            if (notCross)
            {
// #if DEBUG
//
//                 Console.Out.WriteLine($"not cross Leaf Zone {notCross}::: {AabbPackBoxShapes.Count}");
// #endif
                slideTwoDPoint = null;
            }
            else

                slideTwoDPoint = SomeTools.SlideTwoDPoint(AaBbPackBox, line, safe);

            return slideTwoDPoint;
        }

        public void RemoveBlockBox(BlockBox box)
        {
            AaBbPackBox.Remove(box);
        }

        public IEnumerable<BlockBox> TouchBy(BlockBox box)
        {
            return box.TryTouch(AaBbPackBox.OfType<BlockBox>());
        }

        public bool LineIsBlockSight(TwoDVectorLine line)
        {
            var notCross = line.GenZone().RealNotCross(Zone);
            if (notCross) return false;

            return (from aabbPackBoxShape in AaBbPackBox
                let notCross2 = line.GenZone().RealNotCross(aabbPackBoxShape.Zone)
                where !notCross2
                select line.IsSightBlockByWall(aabbPackBoxShape.GetShape())).Any(isTouchAnother => isTouchAnother);
        }

        public void InsertBlockBox(BlockBox box)
        {
            AaBbPackBox.Add(box);
        }

        public IQSpace TryCovToLimitBlockQSpace(int limit)
        {
            if (AaBbPackBox.Count <= limit)
            {
                return this;
            }

            var tryCovToBranch = TryCovToBranch(limit);
            return tryCovToBranch;
        }

        public (int, BlockBox?) TouchWithARightShootPoint(TwoDPoint p)
        {
            return p.GenARightShootCrossALotAabbBoxShapeInQSpace(AaBbPackBox.OfType<BlockBox>());
        }

        public string OutZones()
        {
            var s = "Leaf:::" + AaBbPackBox.Count + "\n";

            return AaBbPackBox.Select(aabbBoxShape => aabbBoxShape.Zone)
                .Aggregate(s, (current, zone) => current + (SomeTools.ZoneLog(zone) + "\n"));
        }

        public int FastTouchWithARightShootPoint(TwoDPoint p)
        {
            var i = p.FastGenARightShootCrossALotAabbBoxShape(AaBbPackBox);
            return i;
        }

        public void ForeachDoWithOutMove<T>(Action<IIdPointShape, T> doWithIIdPointShape, T t)
        {
            foreach (var shape in AaBbPackBox.OfType<IdPointBox>()
                .Select(aabbPackBoxShape => aabbPackBoxShape.IdPointShape))
            {
                doWithIIdPointShape(shape, t);
            }
        }

        public void ForeachDoWithOutMove<T>(Action<IIdPointShape, T> doWithIIdPointShape, T t, Zone zone)
        {
            var idPointShapes = AaBbPackBox.OfType<IdPointBox>().Select(box => box.IdPointShape)
                .Where(x => zone.IncludePt(x.GetAnchor()));

            foreach (var idPointShape in idPointShapes)
            {
                doWithIIdPointShape(idPointShape, t);
            }
        }


        public QSpaceBranch TryCovToBranch(int limit)
        {
            var one = new HashSet<IAaBbBox>();
            var two = new HashSet<IAaBbBox>();
            var three = new HashSet<IAaBbBox>();
            var four = new HashSet<IAaBbBox>();
            var zone = new HashSet<IAaBbBox>();
            var (horizon, vertical) = Zone.GetMid();


            foreach (var intTBoxShapes in AaBbPackBox.SelectMany(aabbBoxShape =>
                aabbBoxShape.SplitByQuads(horizon, vertical)))
            {
                var (i, aabbBoxShape1) = intTBoxShapes;
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


            var zones = Zone.CutTo4(horizon, vertical);

            var qs1 = new QSpaceLeaf(Quad.One, null, zones[0], one);
            var qs2 = new QSpaceLeaf(Quad.Two, null, zones[1], two);
            var qs3 = new QSpaceLeaf(Quad.Three, null, zones[2], three);
            var qs4 = new QSpaceLeaf(Quad.Four, null, zones[3], four);
            var tryCovToBranch = new QSpaceBranch(TheQuad, Father, Zone, zone,
                qs1.TryCovToLimitBlockQSpace(limit),
                qs2.TryCovToLimitBlockQSpace(limit),
                qs3.TryCovToLimitBlockQSpace(limit),
                qs4.TryCovToLimitBlockQSpace(limit));
            tryCovToBranch.QuadOne.Father = tryCovToBranch;
            tryCovToBranch.QuadTwo.Father = tryCovToBranch;
            tryCovToBranch.QuadThree.Father = tryCovToBranch;
            tryCovToBranch.QuadFour.Father = tryCovToBranch;
            return tryCovToBranch;
        }

        public IEnumerable<IIdPointShape> FilterToGIdPsList<T>(Func<IIdPointShape, T, bool> funcWithIIdPtsShape,
            T t, Zone zone)
        {
            return SomeTools.FilterToGIdPsList(this, funcWithIIdPtsShape, t, zone);
        }

        public IEnumerable<T> MapToIEnum<T>(Func<IIdPointShape, T> funcWithIIdPtsShape
        )
        {
            var dicIntToTu = new HashSet<T>();

            void Act(IIdPointShape id, bool a)
            {
                var withIIdPtsShape = funcWithIIdPtsShape(id);
                dicIntToTu.Add(withIIdPtsShape);
            }

            ForeachDoWithOutMove(Act, true);
            return dicIntToTu;
        }

        public HashSet<IBlockShape> GetAllIBlocks()
        {
            var blockShapes =
                AaBbPackBox.OfType<BlockBox>().Select(x => x.Shape).ToList();
            return SomeTools.EnumerableToHashSet(blockShapes);
        }

        public AreaBox? PointInWhichArea(TwoDPoint pt)
        {
            return AaBbPackBox.OfType<AreaBox>().FirstOrDefault(x => x.IsInArea(pt));
        }

        public Dictionary<int, TU> MapToDicGidToSth<TU, T>(Func<IIdPointShape, T, TU> funcWithIIdPtsShape,
            T t)
        {
            return SomeTools.MapToDicGidToSthTool(this, funcWithIIdPtsShape, t);
        }

        public IEnumerable<IIdPointShape> FilterToGIdPsList<T>(Func<IIdPointShape, T, bool> funcWithIIdPtsShape,
            T t)
        {
            return SomeTools.FilterToGIdPsList(this, funcWithIIdPtsShape, t);
        }

        public IQSpace TryCovToLimitAreaQSpace(int limit)
        {
            if (AaBbPackBox.Count <= limit)
            {
                return this;
            }

            var tryCovToBranch = TryCovToBranch(limit);
            return tryCovToBranch;
        }
    }
}