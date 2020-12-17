using System;
using System.Collections.Generic;
using System.Diagnostics;
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


        public QSpaceBranch CovIdPointBranch(HashSet<IAaBbBox>? q1 = null, HashSet<IAaBbBox>? q2 = null,
            HashSet<IAaBbBox>? q3 = null, HashSet<IAaBbBox>? q4 = null)
        {
            var cutTo4 = Zone.CutTo4();
            IQSpace qsl1 = new QSpaceLeaf(Quad.One, null, cutTo4[0], q1 ?? new HashSet<IAaBbBox>());
            IQSpace qsl2 = new QSpaceLeaf(Quad.Two, null, cutTo4[1], q2 ?? new HashSet<IAaBbBox>());
            IQSpace qsl3 = new QSpaceLeaf(Quad.Three, null, cutTo4[2], q3 ?? new HashSet<IAaBbBox>());
            IQSpace qsl4 = new QSpaceLeaf(Quad.Four, null, cutTo4[3], q4 ?? new HashSet<IAaBbBox>());
            var qSpaceBranch = new QSpaceBranch(TheQuad, Father, Zone, AaBbPackBox, qsl1, qsl2,
                qsl3, qsl4);

            if (Father == null) return qSpaceBranch;
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

            return qSpaceBranch;
        }

        public void AddIdPointBoxes(HashSet<IdPointBox> idPointBox, int limit, bool needRecord = false)
        {
            var rawCount = AaBbPackBox.Count;
            if (rawCount + idPointBox.Count <= limit)
            {
                AaBbPackBox.UnionWith(idPointBox);
            }
            else
            {
                var q1 = new HashSet<IAaBbBox>();
                var q2 = new HashSet<IAaBbBox>();
                var q3 = new HashSet<IAaBbBox>();
                var q4 = new HashSet<IAaBbBox>();
                foreach (var pointBox in idPointBox)
                {
                    var shape = pointBox.GetShape();
                    switch (shape)
                    {
                        case IIdPointShape idPointShape:
                            var twoDPoint = idPointShape.GetAnchor();
                            if (AaBbPackBox.Count <= limit)
                            {
                                AaBbPackBox.Add(pointBox);
                            }
                            else
                            {
                                var whichQ = twoDPoint.WhichQ(Zone);
                                if (needRecord) pointBox.AddRecord(whichQ);
                                switch (whichQ)
                                {
                                    case Quad.One:

                                        q1.Add(pointBox);
                                        break;
                                    case Quad.Two:

                                        q2.Add(pointBox);
                                        break;
                                    case Quad.Three:
                                        q3.Add(pointBox);
                                        break;
                                    case Quad.Four:
                                        q4.Add(pointBox);
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

                if (Father == null) return;
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

        public void AddAIdPointBox(IdPointBox idPointBox, int limit, bool needRecord = false)
        {
            var rawCount = AaBbPackBox.Count;
            if (rawCount + 1 <= limit)
            {
                AaBbPackBox.Add(idPointBox);
            }
            else
            {
                var covIdPointBranch = CovIdPointBranch();
                if (idPointBox.GetShape() is IIdPointShape idPointShape)
                {
                    var quad = idPointShape.GetAnchor().WhichQ(Zone);
                    if (needRecord) idPointBox.AddRecord(quad);
                    switch (quad)
                    {
                        case Quad.One:
                            covIdPointBranch.QuadOne.AddAIdPointBox(idPointBox, limit, needRecord);
                            break;
                        case Quad.Two:
                            covIdPointBranch.QuadTwo.AddAIdPointBox(idPointBox, limit, needRecord);
                            break;
                        case Quad.Three:
                            covIdPointBranch.QuadThree.AddAIdPointBox(idPointBox, limit, needRecord);
                            break;
                        case Quad.Four:
                            covIdPointBranch.QuadFour.AddAIdPointBox(idPointBox, limit, needRecord);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void MoveIdPointBoxes(Dictionary<int, ITwoDTwoP> gidToMove, int limit)
        {
            var (inZone, outZone) =
                SomeTools.MovePtsReturnInAndOut(gidToMove, AaBbPackBox.OfType<IdPointBox>(), Zone);

            AaBbPackBox = inZone;
            if (Father != null)
            {
                Father.AddIdPointBoxes(outZone, limit);
            }
            else
            {
                AaBbPackBox.UnionWith(outZone);
            }
        }

        public bool RemoveAIdPointBox(IdPointBox idPointBox)
        {
            return AaBbPackBox.Remove(idPointBox);
        }

        public void RemoveIdPointBoxes(HashSet<IdPointBox> idPointBoxes)
        {
            if (idPointBoxes.Count == 0)
            {
                return;
            }

            foreach (var idPointBox in idPointBoxes)
            {
                AaBbPackBox.Remove(idPointBox);
            }
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

        public IQSpace AddSingleAaBbBox(IAaBbBox aaBbBox, int limit)
        {
            if (AaBbPackBox.Count < limit)
            {
                AaBbPackBox.Add(aaBbBox);
                return this;
            }

            var tryCovToBranch = TryCovToBranch(limit);
            tryCovToBranch.AddSingleAaBbBox(aaBbBox, limit);
            return tryCovToBranch;
        }

        public bool RemoveSingleAaBbBox(IAaBbBox aaBbBox)
        {
            return AaBbPackBox.Remove(aaBbBox);
        }

        public IAaBbBox? InteractiveFirstSingleBox(TwoDPoint pos, Func<IAaBbBox, bool>? filter)
        {
            return AaBbPackBox.FirstOrDefault(x => x.CanInteractive(pos) &&
                                                   (filter == null || filter(x)));
        }

        public void InsertBlockBox(BlockBox box)
        {
            AaBbPackBox.Add(box);
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

        public void ForeachBoxDoWithOutMove<T, TK>(Action<TK, T> action, T t)
        {
            foreach (var shape in AaBbPackBox.OfType<TK>())
            {
                action(shape, t);
            }
        }

        public void ForeachBoxDoWithOutMove<T, TK>(Action<TK, T> action, T t, Zone zone)
        {
            foreach (var shape in AaBbPackBox)
            {
                if (!zone.NotCross(shape.Zone) && shape is TK tt)
                {
                    action(tt, t);
                }
            }
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
                qs1.TryCovToLimitQSpace(limit),
                qs2.TryCovToLimitQSpace(limit),
                qs3.TryCovToLimitQSpace(limit),
                qs4.TryCovToLimitQSpace(limit));
            tryCovToBranch.QuadOne.Father = tryCovToBranch;
            tryCovToBranch.QuadTwo.Father = tryCovToBranch;
            tryCovToBranch.QuadThree.Father = tryCovToBranch;
            tryCovToBranch.QuadFour.Father = tryCovToBranch;

            if (Father == null) return tryCovToBranch;
            switch (TheQuad)
            {
                case Quad.One:
                    Father.QuadOne = tryCovToBranch;
                    break;
                case Quad.Two:
                    Father.QuadOne = tryCovToBranch;
                    break;
                case Quad.Three:
                    Father.QuadOne = tryCovToBranch;
                    break;
                case Quad.Four:
                    Father.QuadOne = tryCovToBranch;
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return tryCovToBranch;
        }

        public IEnumerable<IIdPointShape> FilterToGIdPsList<T>(Func<IIdPointShape, T, bool> funcWithIIdPtsShape,
            T t, Zone zone)
        {
            return SomeTools.FilterToGIdPsList(this, funcWithIIdPtsShape, t, zone);
        }

        public Dictionary<int, T> MapToIDict<T>(Func<IIdPointShape, T> funcWithIIdPtsShape)
        {
            var dicIntToTu = new Dictionary<int, T>();

            void Act(IIdPointShape id, bool a)
            {
                var withIIdPtsShape = funcWithIIdPtsShape(id);
                dicIntToTu[id.GetId()] = withIIdPtsShape;
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

        public IEnumerable<TK> FilterToBoxList<TK, T>(Func<TK, T, bool> func, T t, Zone zone)
        {
            return SomeTools.FilterToBoxList(this, func, t, zone);
        }

        public IEnumerable<IIdPointShape> FilterToGIdPsList<T>(Func<IIdPointShape, T, bool> funcWithIIdPtsShape,
            T t)
        {
            return SomeTools.FilterToGIdPsList(this, funcWithIIdPtsShape, t);
        }

        public IQSpace TryCovToLimitQSpace(int limit)
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