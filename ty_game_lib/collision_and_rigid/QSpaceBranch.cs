using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace collision_and_rigid
{
    public class QSpaceBranch : IQSpace
    {
        public QSpaceBranch(Quad? quad, QSpaceBranch? father, Zone zone, HashSet<AabbBoxShape> aabbPackBoxes,
            ref IQSpace quadOne, ref IQSpace quadTwo,
            ref IQSpace quadThree, ref IQSpace quadFour)
        {
            Father = father;
            TheQuad = quad;
            Zone = zone;
            AabbPackBoxShapes = aabbPackBoxes;
            QuadTwo = quadTwo;
            QuadOne = quadOne;
            QuadFour = quadFour;
            QuadThree = quadThree;

            QuadOne.Father = this;
            QuadTwo.Father = this;
            QuadThree.Father = this;
            QuadFour.Father = this;
        }


        public Quad? TheQuad { get; set; }
        public QSpaceBranch? Father { get; set; }

        public Zone Zone { get; set; }

        public HashSet<AabbBoxShape> AabbPackBoxShapes { get; set; }
        public IQSpace QuadTwo { get; set; }
        public IQSpace QuadOne { get; set; }
        public IQSpace QuadFour { get; set; }
        public IQSpace QuadThree { get; set; }

        public void AddIdPoint(HashSet<AabbBoxShape> idPointShapes, int limit)
        {
            var outZone = new HashSet<AabbBoxShape>();
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

                        if (Zone.IncludePt(twoDPoint))
                        {
                            if (AabbPackBoxShapes.Count <= limit)
                            {
                                AabbPackBoxShapes.Add(aabbBoxShape);
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
            var (inZone, outZone) = SomeTools.MovePtsReturnInAndOut(gidToMove, AabbPackBoxShapes, Zone);
            AabbPackBoxShapes = inZone;

            Father?.AddIdPoint(outZone, limit);

            QuadOne.MoveIdPoint(gidToMove, limit);
            QuadTwo.MoveIdPoint(gidToMove, limit);
            QuadThree.MoveIdPoint(gidToMove, limit);
            QuadFour.MoveIdPoint(gidToMove, limit);
        }


        public TwoDPoint? GetSlidePoint(TwoDVectorLine line, bool isPush, bool safe = true)
        {
            var notCross = Zone.NotCross(line.GenZone());
            if (notCross) return null;

            var a = SomeTools.SlideTwoDPoint(AabbPackBoxShapes, line, isPush, safe);
            if (a != null) return a;

            var qSpaces = new[] {QuadOne, QuadTwo, QuadThree, QuadFour};
            return qSpaces.Select(qSpace => qSpace.GetSlidePoint(line, isPush, safe))
                .FirstOrDefault(twoDPoint => twoDPoint != null);
        }


        public void Remove(AabbBoxShape boxShape)
        {
            if (AabbPackBoxShapes.Remove(boxShape)) return;

            QuadTwo.Remove(boxShape);
            QuadThree.Remove(boxShape);
            QuadFour.Remove(boxShape);
            QuadOne.Remove(boxShape);
        }

        public IEnumerable<AabbBoxShape> TouchBy(AabbBoxShape boxShape)
        {
            var aabbBoxes = boxShape.TryTouch(AabbPackBoxShapes);
            var (item1, item2) = Zone.GetMid();
            var cutTo4 = boxShape.CutTo4(item1, item2);
            foreach (var keyValuePair in cutTo4)
                switch (keyValuePair.Key)
                {
                    case Quad.One:
                        var touchBy = QuadOne.TouchBy(boxShape);
                        aabbBoxes.AddRange(touchBy);
                        break;
                    case Quad.Two:
                        var boxes = QuadTwo.TouchBy(boxShape);
                        aabbBoxes.AddRange(boxes);
                        break;
                    case Quad.Three:
                        var list = QuadThree.TouchBy(boxShape);
                        aabbBoxes.AddRange(list);
                        break;
                    case Quad.Four:
                        var by = QuadFour.TouchBy(boxShape);
                        aabbBoxes.AddRange(by);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            return aabbBoxes;
        }

        public bool LineIsCross(TwoDVectorLine line)
        {
            var notCross = line.GenZone().NotCross(Zone);
            if (notCross) return false;

            return (from aabbPackBoxShape in AabbPackBoxShapes
                let notCross2 = line.GenZone().NotCross(aabbPackBoxShape.Zone)
                where !notCross2
                select line.IsTouchAnother(aabbPackBoxShape.Shape)).Any(isTouchAnother => isTouchAnother);
        }

        public IQSpace TryCovToLimitQSpace(int limit)
        {
            var tryCovToLimitQSpace1 = QuadOne.TryCovToLimitQSpace(limit);
            var tryCovToLimitQSpace2 = QuadTwo.TryCovToLimitQSpace(limit);
            var tryCovToLimitQSpace3 = QuadThree.TryCovToLimitQSpace(limit);
            var tryCovToLimitQSpace4 = QuadFour.TryCovToLimitQSpace(limit);
            return new QSpaceBranch(TheQuad, Father, Zone, AabbPackBoxShapes, ref tryCovToLimitQSpace1,
                ref tryCovToLimitQSpace2,
                ref tryCovToLimitQSpace3, ref tryCovToLimitQSpace4);
        }

        public int FastTouchWithARightShootPoint(TwoDPoint p)
        {
            var i = p.FastGenARightShootCrossALotAabbBoxShape(AabbPackBoxShapes);

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
            foreach (var shape in AabbPackBoxShapes.Select(aabbPackBoxShape => aabbPackBoxShape.Shape))
            {
                switch (shape)
                {
                    case IIdPointShape idPointShape:
                        doWithIIdPointShape(idPointShape, t);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(shape));
                }
            }

            QuadOne.ForeachDoWithOutMove(doWithIIdPointShape, t);
            QuadTwo.ForeachDoWithOutMove(doWithIIdPointShape, t);
            QuadThree.ForeachDoWithOutMove(doWithIIdPointShape, t);
            QuadFour.ForeachDoWithOutMove(doWithIIdPointShape, t);
        }


        public (int, AabbBoxShape?) TouchWithARightShootPoint(TwoDPoint p)
        {
            var (i, aabb) = p.GenARightShootCrossALotAabbBoxShape(AabbPackBoxShapes);
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
            var s = "Branch:::";
            foreach (var aabbBoxShape in AabbPackBoxShapes)
            {
                var zone = aabbBoxShape.Zone;
                s += SomeTools.ZoneLog(zone) + "\n";
            }

            var qSpaces = new[] {QuadOne, QuadTwo, QuadThree, QuadFour};
            foreach (var qSpace in qSpaces)
            {
                var zones = qSpace.OutZones();
                s += zones + "\n";
            }

            return s;
        }

        public void InsertBox(AabbBoxShape boxShape)
        {
            var (item1, item2) = Zone.GetMid();
            var cutTo4 = boxShape.WhichQ(item1, item2);
            switch (cutTo4)
            {
                case Quad.One:
                    QuadTwo.InsertBox(boxShape);
                    break;
                case Quad.Two:
                    QuadOne.InsertBox(boxShape);
                    break;
                case Quad.Three:
                    QuadFour.InsertBox(boxShape);
                    break;
                case Quad.Four:
                    QuadThree.InsertBox(boxShape);
                    break;
                case null:
                    AabbPackBoxShapes.Add(boxShape);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public QSpaceLeaf CovToLeaf()
        {
            var qSpaces = new[] {QuadTwo, QuadOne, QuadFour, QuadThree};
            var a = AabbPackBoxShapes;
            foreach (var qSpace in qSpaces) a.UnionWith(qSpace.AabbPackBoxShapes);

            return new QSpaceLeaf(TheQuad, Father, Zone, a);
        }

        public void TellFather()
        {
            QuadOne.Father = this;
            QuadTwo.Father = this;
            QuadThree.Father = this;
            QuadFour.Father = this;
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

        public Dictionary<int, TU> MapToDicGidToSth<TU, T>(Func<IIdPointShape, T, TU> funcWithIIdPtsShape,
            T t)
        {
            var dicIntToTu = new Dictionary<int, TU>();

            void Act(IIdPointShape id, T tt)
            {
                var withIIdPtsShape = funcWithIIdPtsShape(id, tt);
                if (withIIdPtsShape == null) return;
                var i = id.GetId();

                dicIntToTu[i] = withIIdPtsShape;
            }

            ForeachDoWithOutMove(Act, t);
            return dicIntToTu;
        }

        public IEnumerable<IIdPointShape> FilterToGIdPsList<T>(Func<IIdPointShape, T, bool> funcWithIIdPtsShape,
            T t)
        {
            var dicIntToTu = new HashSet<IIdPointShape>();

            void Act(IIdPointShape id, T tt)
            {
                var withIIdPtsShape = funcWithIIdPtsShape(id, tt);

                if (withIIdPtsShape) dicIntToTu.Add(id);
            }

            ForeachDoWithOutMove(Act, t);
            return dicIntToTu;
        }
    }
}