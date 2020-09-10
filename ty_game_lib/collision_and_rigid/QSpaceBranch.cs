using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

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

        public HashSet<IAaBbBox> AaBbPackBox { get; set; }
        public IQSpace QuadTwo { get; set; }
        public IQSpace QuadOne { get; set; }
        public IQSpace QuadFour { get; set; }
        public IQSpace QuadThree { get; set; }

        public void AddIdPoint(HashSet<IdPointBox> idPointShapes, int limit)
        {
            var outZone = new HashSet<IdPointBox>();
            var q1 = new HashSet<IdPointBox>();
            var q2 = new HashSet<IdPointBox>();
            var q3 = new HashSet<IdPointBox>();
            var q4 = new HashSet<IdPointBox>();
            foreach (var aabbBoxShape in idPointShapes)
            {
                var shape = aabbBoxShape.GetShape();
                switch (shape)
                {
                    case IIdPointShape idPointShape:
                        var twoDPoint = idPointShape.GetAnchor();

                        if (Zone.IncludePt(twoDPoint))
                        {
                            if (AaBbPackBox.Count <= limit)
                            {
                                AaBbPackBox.Add(aabbBoxShape);
                            }
                            else
                            {
                                var whichQ = twoDPoint.WhichQ(this);
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
                        }

                        else
                        {
                            outZone.Add(aabbBoxShape);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(shape));
                }
            }

            Father?.AddIdPoint(outZone, limit);


            QuadOne.AddIdPoint(q1, limit);

            QuadTwo.AddIdPoint(q2, limit);

            QuadThree.AddIdPoint(q3, limit);

            QuadFour.AddIdPoint(q4, limit);
        }

        public void MoveIdPoint(Dictionary<int, ITwoDTwoP> gidToMove, int limit)
        {
            var (inZone, outZone) = SomeTools.MovePtsReturnInAndOut(gidToMove, AaBbPackBox.OfType<IdPointBox>(), Zone);
            AaBbPackBox = inZone;

            if (Father != null)
            {
                Father.AddIdPoint(outZone, limit);
            }
            else
            {
                AaBbPackBox.UnionWith(outZone);
            }

            QuadOne.MoveIdPoint(gidToMove, limit);
            QuadTwo.MoveIdPoint(gidToMove, limit);
            QuadThree.MoveIdPoint(gidToMove, limit);
            QuadFour.MoveIdPoint(gidToMove, limit);
        }

        public void RemoveIdPoint(IdPointBox idPointBox)
        {
            var remove = AaBbPackBox.Remove(idPointBox);
            if (remove) return;
            QuadOne.RemoveIdPoint(idPointBox);
            QuadTwo.RemoveIdPoint(idPointBox);
            QuadThree.RemoveIdPoint(idPointBox);
            QuadFour.RemoveIdPoint(idPointBox);
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
            var allIBlocks = QuadOne.GetAllIBlocks();
            var iBlocks = QuadTwo.GetAllIBlocks();
            var hashSet = QuadThree.GetAllIBlocks();
            var blocks = QuadFour.GetAllIBlocks();
            var enumerable = blockShapes.Union(allIBlocks).Union(iBlocks).Union(hashSet).Union(blocks).ToList();
            return SomeTools.EnumerableToHashSet(enumerable);
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

        public IEnumerable<IIdPointShape> FilterToGIdPsList<T>(Func<IIdPointShape, T, bool> funcWithIIdPtsShape,
            T t)
        {
            return SomeTools.FilterToGIdPsList(this, funcWithIIdPtsShape, t);
        }
    }
}