using System;
using System.Collections.Generic;
using System.Linq;

namespace collision_and_rigid
{
    public class QSpaceBranch : IQSpace
    {
        public QSpaceBranch(Quad? quad, QSpaceBranch? father, Zone zone, HashSet<IAaBbBox> aabbPackBoxes,
            IQSpace quadOne, IQSpace quadTwo,
            IQSpace quadThree, IQSpace quadFour)
        {
            Father = father;
            TheQuad = quad;
            Zone = zone;
            AaBbPackBox = aabbPackBoxes;
            QuadTwo = quadTwo;
            QuadOne = quadOne;
            QuadFour = quadFour;
            QuadThree = quadThree;

            QuadOne.Father = this;
            QuadTwo.Father = this;
            QuadThree.Father = this;
            QuadFour.Father = this;
        }


        public int Count()
        {
            return AaBbPackBox.Count + QuadOne.Count() + QuadFour.Count() + QuadThree.Count() + QuadTwo.Count();
        }

        public Quad? TheQuad { get; set; }
        public QSpaceBranch? Father { get; set; }

        public Zone Zone { get; set; }

        public HashSet<IAaBbBox> AaBbPackBox { get; private set; }
        public IQSpace QuadTwo { get; set; }
        public IQSpace QuadOne { get; set; }
        public IQSpace QuadFour { get; set; }
        public IQSpace QuadThree { get; set; }

        public void AddIdPointBoxes(HashSet<IdPointBox> idPointBox, int limit, bool needRecord = false)
        {
            var outZone = new HashSet<IdPointBox>();
            var q1 = new HashSet<IdPointBox>();
            var q2 = new HashSet<IdPointBox>();
            var q3 = new HashSet<IdPointBox>();
            var q4 = new HashSet<IdPointBox>();
            foreach (var pointBox in idPointBox)
            {
                var shape = pointBox.GetShape();
                switch (shape)
                {
                    case IIdPointShape idPointShape:
                        var twoDPoint = idPointShape.GetAnchor();

                        if (Zone.IncludePt(twoDPoint))
                        {
                            if (AaBbPackBox.Count <= limit)
                            {
                                AaBbPackBox.Add(pointBox);
                            }
                            else
                            {
                                var whichQ = twoDPoint.WhichQ(this);
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
                        }

                        else
                        {
                            outZone.Add(pointBox);
                            if (needRecord) pointBox.RemoveLastRecord();
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(shape));
                }
            }

            Father?.AddIdPointBoxes(outZone, limit);

            QuadOne.AddIdPointBoxes(q1, limit);

            QuadTwo.AddIdPointBoxes(q2, limit);

            QuadThree.AddIdPointBoxes(q3, limit);

            QuadFour.AddIdPointBoxes(q4, limit);
        }

        public void AddAIdPointBox(IdPointBox idPointBox, int limit, bool needRecord = false)
        {
            var shape = idPointBox.GetShape();
            switch (shape)
            {
                case IIdPointShape idPointShape:
                    var twoDPoint = idPointShape.GetAnchor();

                    if (Zone.IncludePt(twoDPoint))
                    {
                        if (AaBbPackBox.Count <= limit)
                        {
                            AaBbPackBox.Add(idPointBox);
                        }
                        else
                        {
                            var whichQ = twoDPoint.WhichQ(this);
                            if (needRecord) idPointBox.AddRecord(whichQ);
                            switch (whichQ)
                            {
                                case Quad.One:
                                    QuadOne.AddAIdPointBox(idPointBox, limit, needRecord);
                                    break;
                                case Quad.Two:
                                    QuadTwo.AddAIdPointBox(idPointBox, limit, needRecord);
                                    break;
                                case Quad.Three:
                                    QuadThree.AddAIdPointBox(idPointBox, limit, needRecord);
                                    break;
                                case Quad.Four:
                                    QuadFour.AddAIdPointBox(idPointBox, limit, needRecord);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }

                    else
                    {
                        if (Father == null)
                            AaBbPackBox.Add(idPointBox);
                        else Father.AddAIdPointBox(idPointBox, limit, needRecord);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shape));
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

            QuadOne.MoveIdPointBoxes(gidToMove, limit);
            QuadTwo.MoveIdPointBoxes(gidToMove, limit);
            QuadThree.MoveIdPointBoxes(gidToMove, limit);
            QuadFour.MoveIdPointBoxes(gidToMove, limit);
        }

        public bool RemoveAIdPointBox(IdPointBox idPointBox)
        {
            var readRecord = idPointBox.ReadRecord();

            switch (readRecord)
            {
                case Quad.One:
                    return QuadOne.RemoveAIdPointBox(idPointBox);
                case Quad.Two:
                    return QuadTwo.RemoveAIdPointBox(idPointBox);
                case Quad.Three:
                    return QuadThree.RemoveAIdPointBox(idPointBox);
                case Quad.Four:
                    return QuadFour.RemoveAIdPointBox(idPointBox);
                case null:
                    var remove = AaBbPackBox.Remove(idPointBox);
                    if (remove) return true;
                    return QuadOne.RemoveAIdPointBox(idPointBox)
                           || QuadTwo.RemoveAIdPointBox(idPointBox)
                           || QuadThree.RemoveAIdPointBox(idPointBox)
                           || QuadFour.RemoveAIdPointBox(idPointBox);


                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IEnumerable<IdPointBox> RemoveIdPointBoxes(IEnumerable<IdPointBox> idPointBoxes)
        {
            var removeIdPointBoxes = idPointBoxes as IdPointBox[] ?? idPointBoxes.ToArray();
            if (!removeIdPointBoxes.Any())
            {
                return removeIdPointBoxes;
            }

            var aaBbBoxes = removeIdPointBoxes.Where(AaBbPackBox.Contains).ToArray();
            var i = aaBbBoxes.Length;
            AaBbPackBox.ExceptWith(aaBbBoxes);
            var except = removeIdPointBoxes.Except(aaBbBoxes);

            var groupBy = except.GroupBy(x => x.GetAnchor().WhichQ(this));

            return groupBy.SelectMany(gg =>
            {
                return gg.Key switch
                {
                    Quad.One => QuadOne.RemoveIdPointBoxes(gg),
                    Quad.Two => QuadTwo.RemoveIdPointBoxes(gg),
                    Quad.Three => QuadThree.RemoveIdPointBoxes(gg),
                    Quad.Four => QuadFour.RemoveIdPointBoxes(gg),
                    _ => throw new ArgumentOutOfRangeException()
                };
            });
        }


        public TwoDPoint? GetSlidePoint(TwoDVectorLine line, bool safe = true)
        {
            var notCross = Zone.RealNotCross(line.GenZone());

            if (notCross) return null;

            var a = SomeTools.SlideTwoDPoint(AaBbPackBox, line, safe);
            if (a != null) return a;
// #if DEBUG
//
//             Console.Out.WriteLine($"not cross Branch Zone {notCross}::: {AabbPackBoxShapes.Count}");
// #endif
            var qSpaces = new[] {QuadOne, QuadTwo, QuadThree, QuadFour};
            return qSpaces.Select(qSpace => qSpace.GetSlidePoint(line, safe))
                .FirstOrDefault(twoDPoint => twoDPoint != null);
        }


        public void RemoveBlockBox(BlockBox box)
        {
            if (AaBbPackBox.Remove(box)) return;

            QuadTwo.RemoveBlockBox(box);
            QuadThree.RemoveBlockBox(box);
            QuadFour.RemoveBlockBox(box);
            QuadOne.RemoveBlockBox(box);
        }

        public IEnumerable<BlockBox> TouchBy(BlockBox box)
        {
            var aabbBoxes = box.TryTouch(AaBbPackBox.OfType<BlockBox>());
            var (item1, item2) = Zone.GetMid();
            var cutTo4 = box.CutTo4(item1, item2);
            foreach (var keyValuePair in cutTo4)
                switch (keyValuePair.Key)
                {
                    case Quad.One:
                        var touchBy = QuadOne.TouchBy(box);
                        aabbBoxes.AddRange(touchBy);
                        break;
                    case Quad.Two:
                        var boxes = QuadTwo.TouchBy(box);
                        aabbBoxes.AddRange(boxes);
                        break;
                    case Quad.Three:
                        var list = QuadThree.TouchBy(box);
                        aabbBoxes.AddRange(list);
                        break;
                    case Quad.Four:
                        var by = QuadFour.TouchBy(box);
                        aabbBoxes.AddRange(by);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            return aabbBoxes;
        }

        public bool LineIsBlockSight(TwoDVectorLine line)
        {
            var notCross = line.GenZone().RealNotCross(Zone);
            if (notCross) return false;

            var lineIsBlockSight = (from aabbPackBoxShape in AaBbPackBox
                let notCross2 = line.GenZone().RealNotCross(aabbPackBoxShape.Zone)
                where !notCross2
                select line.IsSightBlockByWall(aabbPackBoxShape.GetShape())).Any(isTouchAnother => isTouchAnother);

            var isBlockSight = QuadOne.LineIsBlockSight(line) ||
                               QuadTwo.LineIsBlockSight(line) ||
                               QuadThree.LineIsBlockSight(line) ||
                               QuadFour.LineIsBlockSight(line);
            return lineIsBlockSight || isBlockSight;
        }

        public void AddSingleAaBbBox(IAaBbBox aaBbBox, int limit)
        {
            if (AaBbPackBox.Count < limit)
            {
                AaBbPackBox.Add(aaBbBox);
                return;
            }

            var (horizon, vertical) = Zone.GetMid();
            var splitByQuads = aaBbBox.SplitByQuads(horizon, vertical);
            foreach (var splitByQuad in splitByQuads)
            {
                var (item1, item2) = splitByQuad;
                switch (item1)
                {
                    case 0:
                        AaBbPackBox.Add(item2);
                        break;
                    case 1:
                        QuadOne.AddSingleAaBbBox(item2, limit);
                        break;
                    case 2:
                        QuadTwo.AddSingleAaBbBox(item2, limit);
                        break;
                    case 3:
                        QuadThree.AddSingleAaBbBox(item2, limit);
                        break;
                    case 4:
                        QuadFour.AddSingleAaBbBox(item2, limit);
                        break;
                }
            }
        }

        public void AddRangeAabbBoxes(HashSet<IAaBbBox> aaBbBoxes, int limit)
        {
            var intPtr = AaBbPackBox.Count + aaBbBoxes.Count;
            if (intPtr <= limit)
            {
                AaBbPackBox.UnionWith(aaBbBoxes);
                return;
            }

            var count = limit - AaBbPackBox.Count;

            var bbBoxes = aaBbBoxes.Take(count).ToList();
            AaBbPackBox.UnionWith(bbBoxes);
            aaBbBoxes.ExceptWith(bbBoxes);

            var (horizon, vertical) = Zone.GetMid();
            var splitByQuads = aaBbBoxes.SelectMany(box => box.SplitByQuads(horizon, vertical)).GroupBy(x => x.Item1);

            foreach (var splitByQuad in splitByQuads)
            {
                var enumerable = splitByQuad.Select(tuple => tuple.Item2);
                var enumerableToHashSet = SomeTools.IeToHashSet(enumerable);
                switch (splitByQuad.Key)
                {
                    case 0:
                        AaBbPackBox.UnionWith(enumerableToHashSet);
                        break;
                    case 1:

                        QuadOne.AddRangeAabbBoxes(enumerableToHashSet, limit);
                        break;
                    case 2:
                        QuadTwo.AddRangeAabbBoxes(enumerableToHashSet, limit);
                        break;
                    case 3:
                        QuadThree.AddRangeAabbBoxes(enumerableToHashSet, limit);
                        break;
                    case 4:
                        QuadFour.AddRangeAabbBoxes(enumerableToHashSet, limit);
                        break;
                }
            }
        }

        public bool RemoveSingleAaBbBox(IAaBbBox aaBbBox)
        {
            if (AaBbPackBox.Remove(aaBbBox)) return true;
            var nextQuad = aaBbBox.GetNextQuad();
            return nextQuad switch
            {
                Quad.One => QuadOne.RemoveSingleAaBbBox(aaBbBox),
                Quad.Two => QuadTwo.RemoveSingleAaBbBox(aaBbBox),
                Quad.Three => QuadThree.RemoveSingleAaBbBox(aaBbBox),
                Quad.Four => QuadFour.RemoveSingleAaBbBox(aaBbBox),
                null => QuadOne.RemoveSingleAaBbBox(aaBbBox) || QuadTwo.RemoveSingleAaBbBox(aaBbBox) ||
                        QuadThree.RemoveSingleAaBbBox(aaBbBox) || QuadFour.RemoveSingleAaBbBox(aaBbBox),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public IAaBbBox? InteractiveFirstSingleBox(TwoDPoint pos, Func<IAaBbBox, bool>? filter)
        {
            var firstOrDefault =
                AaBbPackBox.FirstOrDefault(x => x.CanInteractive(pos));
            if (firstOrDefault != null)
            {
                return firstOrDefault;
            }

            var whichQ = pos.WhichQ(this);
            return whichQ switch
            {
                Quad.One => QuadOne.InteractiveFirstSingleBox(pos, filter),
                Quad.Two => QuadTwo.InteractiveFirstSingleBox(pos, filter),
                Quad.Three => QuadThree.InteractiveFirstSingleBox(pos, filter),
                Quad.Four => QuadFour.InteractiveFirstSingleBox(pos, filter),
                _ => throw new ArgumentOutOfRangeException()
            };
        }


        public int FastTouchWithARightShootPoint(TwoDPoint p)
        {
            var i = p.FastGenARightShootCrossALotAabbBoxShape(AaBbPackBox);

            var qSpaces = new[] {QuadOne, QuadTwo};
            var dqSpace = new[] {QuadThree, QuadFour};
            var m = (Zone.Up + Zone.Down) / 2;
            var b = p.Y > m;
            if (b)
                i += qSpaces.Sum(qSpace => qSpace.FastTouchWithARightShootPoint(p));
            else
                i += dqSpace.Sum(qSpace => qSpace.FastTouchWithARightShootPoint(p));

            return i;
        }

        public void ForeachBoxDoWithOutMove<T, TK>(Action<TK, T> action, T t)
        {
            foreach (var shape in AaBbPackBox.OfType<TK>())
            {
                action(shape, t);
            }

            QuadOne.ForeachBoxDoWithOutMove(action, t);
            QuadTwo.ForeachBoxDoWithOutMove(action, t);
            QuadThree.ForeachBoxDoWithOutMove(action, t);
            QuadFour.ForeachBoxDoWithOutMove(action, t);
        }

        public void ForeachBoxDoWithOutMove<T, TK>(Action<TK, T> action, T t, Zone zone)
        {
            foreach (var shape in AaBbPackBox.OfType<TK>())
            {
                action(shape, t);
            }

            var valueTuples = Zone.SplitZones(zone);
            foreach (var (quad, zone1) in valueTuples)
            {
                switch (quad)
                {
                    case Quad.One:
                        QuadOne.ForeachBoxDoWithOutMove(action, t, zone1);
                        break;
                    case Quad.Two:
                        QuadTwo.ForeachBoxDoWithOutMove(action, t, zone1);
                        break;
                    case Quad.Three:
                        QuadThree.ForeachBoxDoWithOutMove(action, t, zone1);
                        break;
                    case Quad.Four:
                        QuadFour.ForeachBoxDoWithOutMove(action, t, zone1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void ForeachDoWithOutMove<T>(Action<IIdPointShape, T> doWithIIdPointShape, T t)
        {
            foreach (var shape in AaBbPackBox.OfType<IdPointBox>()
                .Select(boxShape => boxShape.IdPointShape))
            {
                doWithIIdPointShape(shape, t);
            }

            QuadOne.ForeachDoWithOutMove(doWithIIdPointShape, t);
            QuadTwo.ForeachDoWithOutMove(doWithIIdPointShape, t);
            QuadThree.ForeachDoWithOutMove(doWithIIdPointShape, t);
            QuadFour.ForeachDoWithOutMove(doWithIIdPointShape, t);
        }

        public void ForeachDoWithOutMove<T>(Action<IIdPointShape, T> doWithIIdPointShape, T t, Zone zone)
        {
            var idPointShapes = AaBbPackBox.OfType<IdPointBox>().Select(x => x.IdPointShape)
                .Where(x => Zone.IncludePt(x.GetAnchor()));
            foreach (var idPointShape in idPointShapes)
            {
                doWithIIdPointShape(idPointShape, t);
            }

            var valueTuples = Zone.SplitZones(zone);
            foreach (var (quad, zone1) in valueTuples)
            {
                switch (quad)
                {
                    case Quad.One:
                        QuadOne.ForeachDoWithOutMove(doWithIIdPointShape, t, zone1);
                        break;
                    case Quad.Two:
                        QuadTwo.ForeachDoWithOutMove(doWithIIdPointShape, t, zone1);
                        break;
                    case Quad.Three:
                        QuadThree.ForeachDoWithOutMove(doWithIIdPointShape, t, zone1);
                        break;
                    case Quad.Four:
                        QuadFour.ForeachDoWithOutMove(doWithIIdPointShape, t, zone1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        public (int, BlockBox?) TouchWithARightShootPoint(TwoDPoint p)
        {
            var (i, aabb) = p.GenARightShootCrossALotAabbBoxShapeInQSpace(AaBbPackBox.OfType<BlockBox>());
            var whichQ = p.WhichQ(this);
//            Console.Out.WriteLine("Q:::" + whichQ);
            switch (whichQ)
            {
                case Quad.One:
                    var (item1, aabbBoxShape) = QuadOne.TouchWithARightShootPoint(p);
                    if (aabbBoxShape != null) aabb = aabbBoxShape;

                    if (item1 < 0)
                    {
                        i = item1;
                        return (i, aabb);
                    }
                    else
                    {
                        i += item1;
                    }

                    break;
                case Quad.Two:
                    var (item12, aabbBoxShape2) = QuadTwo.TouchWithARightShootPoint(p);
                    if (aabbBoxShape2 != null) aabb = aabbBoxShape2;

                    if (item12 < 0)
                    {
                        i = item12;
                        return (i, aabb);
                    }
                    else
                    {
                        i += item12;
                        i += QuadOne.FastTouchWithARightShootPoint(p);
                    }

                    break;
                case Quad.Three:
                    var (item13, aabbBoxShape3) = QuadThree.TouchWithARightShootPoint(p);
                    if (aabbBoxShape3 != null) aabb = aabbBoxShape3;

                    if (item13 < 0)
                    {
                        i = item13;
                        return (i, aabb);
                    }
                    else
                    {
                        i += item13;
                        i += QuadFour.FastTouchWithARightShootPoint(p);
                    }

                    break;
                case Quad.Four:
                    var (item11, aabbBoxShape4) = QuadFour.TouchWithARightShootPoint(p);
                    if (aabbBoxShape4 != null) aabb = aabbBoxShape4;

                    if (item11 < 0)
                    {
                        i = item11;
                        return (i, aabb);
                    }
                    else
                    {
                        i += item11;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (i, aabb);
        }

        public string OutZones()
        {
            var s = AaBbPackBox.Select(aabbBoxShape => aabbBoxShape.Zone).Aggregate("Branch:::\n",
                (current, zone) => current + (SomeTools.ZoneLog(zone) + "\n"));

            var qSpaces = new[] {QuadOne, QuadTwo, QuadThree, QuadFour};

            return qSpaces.Select(qSpace => qSpace.OutZones())
                .Aggregate(s, (current, zones) => current + (zones + "\n"));
        }

        public void InsertBlockBox(BlockBox box)
        {
            var (item1, item2) = Zone.GetMid();
            var cutTo4 = box.WhichQ(item1, item2);
            switch (cutTo4)
            {
                case Quad.One:
                    QuadTwo.InsertBlockBox(box);
                    break;
                case Quad.Two:
                    QuadOne.InsertBlockBox(box);
                    break;
                case Quad.Three:
                    QuadFour.InsertBlockBox(box);
                    break;
                case Quad.Four:
                    QuadThree.InsertBlockBox(box);
                    break;
                case null:
                    AaBbPackBox.Add(box);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public QSpaceLeaf CovToLeaf()
        {
            var qSpaces = new[] {QuadTwo, QuadOne, QuadFour, QuadThree};
            var a = AaBbPackBox;
            foreach (var qSpace in qSpaces) a.UnionWith(qSpace.AaBbPackBox);

            return new QSpaceLeaf(TheQuad, Father, Zone, a);
        }

        public void TellFather()
        {
            QuadOne.Father = this;
            QuadTwo.Father = this;
            QuadThree.Father = this;
            QuadFour.Father = this;
        }

        public IEnumerable<IIdPointShape> FilterToGIdPsList<T>(Func<IIdPointShape, T, bool> funcWithIIdPtsShape,
            T t, Zone zone)
        {
            return SomeTools.FilterToGIdPsList(this, funcWithIIdPtsShape, t, zone);
        }

        public Dictionary<int, T> MapToIDict<T>(Func<IIdPointShape, T> funcWithIIdPtsShape
        )
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
            var allIBlocks = QuadOne.GetAllIBlocks();
            var iBlocks = QuadTwo.GetAllIBlocks();
            var hashSet = QuadThree.GetAllIBlocks();
            var blocks = QuadFour.GetAllIBlocks();
            var enumerable = blockShapes.Union(allIBlocks).Union(iBlocks).Union(hashSet).Union(blocks).ToList();
            return SomeTools.IeToHashSet(enumerable);
        }

        public AreaBox? PointInWhichArea(TwoDPoint pt)
        {
            var firstOrDefault = AaBbPackBox.OfType<AreaBox>().FirstOrDefault(box => box.IsInArea(pt));
            if (firstOrDefault != null)
            {
                return firstOrDefault;
            }

            var qSpaces = new[]
            {
                QuadOne, QuadTwo, QuadThree, QuadFour
            };
            return qSpaces.Select(qSpace => qSpace.PointInWhichArea(pt))
                .FirstOrDefault(pointInWhichArea => pointInWhichArea != null);
        }

        public Dictionary<int, TU> MapToDicGidToSth<TU, T>(Func<IIdPointShape, T, TU> funcWithIIdPtsShape,
            T t)
        {
            return SomeTools.MapToDicGidToSthTool(this, funcWithIIdPtsShape, t);
        }

        public Dictionary<int, TU> MapToDicGidToSth<TU, T>(Func<IIdPointShape, T, TU> funcWithIIdPtsShape, T t,
            Zone zone)
        {
            return SomeTools.MapToDicGidToSthTool(this, funcWithIIdPtsShape, t, zone);
        }

        public IEnumerable<TU> MapToIEnumNotNullSth<TU, T>(Func<IIdPointShape, T, TU> funcWithIIdPtsShape, T t,
            Zone zone)
        {
            return SomeTools.MapToIEnumSthTool(this, funcWithIIdPtsShape, t, zone);
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
    }
}