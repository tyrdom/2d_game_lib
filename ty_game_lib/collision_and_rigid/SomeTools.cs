using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Xsl;

namespace collision_and_rigid
{
    public static class SomeTools
    {
        public static TwoDPoint[] SwapPoints(TwoDPoint[] x, int a, int b)
        {
            var xLength = x.Length;
            if (a >= xLength || b >= xLength) return x;
            var t = x[b];
            x[b] = x[a];
            x[a] = t;
            return x;
        }


        public static Zone JoinAabbZone(AabbBoxShape[] aabbBoxShapes)
        {
            var foo = aabbBoxShapes[0].Zone;
            foreach (var i in Enumerable.Range(1, aabbBoxShapes.Length - 1))
            {
                foo = foo.Join(aabbBoxShapes[i].Zone);
            }

            return foo;
        }

        public static void LogZones(AabbBoxShape[] aabbBoxShapes)
        {
            string s = "";
            foreach (var aabbBoxShape in aabbBoxShapes)
            {
                var zone = aabbBoxShape.Zone;
                var zoneLog = ZoneLog(zone);
                s += zoneLog + "\n";
            }

            Console.Out.WriteLine("aabb::" + s + "::bbaa");
        }

        public static TwoDPoint? SlideTwoDPoint(List<AabbBoxShape> aabbBoxShapes, TwoDVectorLine line, bool isPush)
        {
            foreach (var aabbBoxShape in aabbBoxShapes)
            {
                var notCross = line.GenZone().NotCross(aabbBoxShape.Zone);
                if (notCross) continue;
                switch (aabbBoxShape.Shape)
                {
                    case ClockwiseTurning blockClockwiseTurning:
                        var isCross = blockClockwiseTurning.IsCross(line);
                        if (isCross)
                        {
                            var twoDPoint = blockClockwiseTurning.Slide(isPush ? line.B : line.A);
                            return twoDPoint;
                        }

                        break;
                    case TwoDVectorLine blockLine:
                        var isCrossAnother = blockLine.SimpleIsCross(line);
                        if (isCrossAnother)
                        {
                            var twoDPoint = blockLine.Slide(isPush ? line.B : line.A);
                            return twoDPoint;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return null;
        }


        public static string ZoneLog(Zone zone)
        {
            return zone.Up + "|" + zone.Down + "|" + zone.Left + "|" + zone.Right;
        }


        static (List<IBlockShape>, bool) AddTwoBlocks(List<IBlockShape> l1, bool l1IsBlockIn, List<IBlockShape> l2,
            bool l2IsBlockIn)
        {
            //record points
            var dicL1 = new Dictionary<int, List<(TwoDPoint, CondAfterCross)>>();
            var dicL2 = new Dictionary<int, List<(TwoDPoint, CondAfterCross)>>();

            for (int i = 0; i < l1.Count; i++)
            {
                var blockShapeFromL1 = l1[i];
                dicL1.TryGetValue(i, out var df1);

                for (int j = 0; j < l2.Count; j++)
                {
                    List<(TwoDPoint, CondAfterCross, CondAfterCross)> crossAnotherBlockShapeReturnIsChangeAndBlocks =
                        blockShapeFromL1.CrossAnotherBlockShapeReturnCrossPtAndThisCondAnotherCond(l2[j]);
                    dicL2.TryGetValue(j, out var df2);
                    if (crossAnotherBlockShapeReturnIsChangeAndBlocks.Count > 0)
                    {
                        foreach (var (twoDPoint, item2, item3) in crossAnotherBlockShapeReturnIsChangeAndBlocks)
                        {
                            if (df1 != null)
                                df1.Add((twoDPoint, item2));
                            else
                                df1 = new List<(TwoDPoint, CondAfterCross)> {(twoDPoint, item2)};

                            if (df2 != null)
                                df2.Add((twoDPoint, item3));
                            else
                                df1 = new List<(TwoDPoint, CondAfterCross)> {(twoDPoint, item3)};
                            ;
                        }

                        dicL2[j] = df2;
                    }

                    if (df1 != null)
                    {
                        dicL1[i] = df1;
                    }
                }
            }

            //

            var resL1 = new List<IBlockShape>();
            var resL2 = new List<IBlockShape>();

            var l1StartPt = l1[0].GetStartPt();
            var l2StartPt = l2[0].GetStartPt();

            var l1QSpace = CreateQSpaceByAabbBoxShapes(Poly.GenBlockAabbBoxShapes(l1).ToArray(), 100);

            var block1 = new WalkBlock(l1IsBlockIn, l1QSpace);
            var l2QSpace = CreateQSpaceByAabbBoxShapes(Poly.GenBlockAabbBoxShapes(l2).ToArray(), 100);

            var block2 = new WalkBlock(l2IsBlockIn, l2QSpace);
            var l2StartCond = block1.CoverPoint(l2StartPt) ? CondAfterCross.OutToIn : CondAfterCross.InToOut;
            var l1StartCond = block2.CoverPoint(l1StartPt) ? CondAfterCross.OutToIn : CondAfterCross.InToOut;

            var nowL1Cond = l1StartCond;
            var nowL2Cond = l2StartCond;
            var temp1 = new List<IBlockShape>();
            var temp2 = new List<IBlockShape>();
            for (int i = 0; i < l1.Count; i++)
            {
                var blockShape = l1[i];

                dicL1.TryGetValue(i, out var ptsAndCond);
                if (ptsAndCond == null)
                {
                    ptsAndCond = new List<(TwoDPoint, CondAfterCross)>();
                }

                var endPt = blockShape.GetEndPt();
                var b = block2.CoverPoint(endPt);
                ptsAndCond.Add(b ? (endPt, CondAfterCross.OutToIn) : (endPt, CondAfterCross.InToOut));

                var (blockShapes, condAfterCross, item3) =
                    blockShape.CutByPointReturnGoodBlockCondAndTemp(nowL1Cond, ptsAndCond, temp1);
                resL1.AddRange(blockShapes);
                nowL1Cond = condAfterCross;
                temp1 = item3;
            }


            for (int i = 0; i < l2.Count; i++)
            {
                var blockShape = l2[i];

                dicL2.TryGetValue(i, out var ptsAndCond2);
                if (ptsAndCond2 == null)
                {
                    ptsAndCond2 = new List<(TwoDPoint, CondAfterCross)>();
                }

                var endPt = blockShape.GetEndPt();
                var b = block1.CoverPoint(endPt);
                ptsAndCond2.Add(b ? (endPt, CondAfterCross.OutToIn) : (endPt, CondAfterCross.InToOut));

                var (blockShapes, condAfterCross, item3) =
                    blockShape.CutByPointReturnGoodBlockCondAndTemp(nowL2Cond, ptsAndCond2, temp2);
                resL2.AddRange(blockShapes);
                nowL2Cond = condAfterCross;
                temp2 = item3;
            }

            resL1.AddRange(resL2);

            return (resL1.Where(x => !x.IsEmpty()).ToList(), l1IsBlockIn && l2IsBlockIn);
        }


        public static List<IBlockShape> CutInSingleShapeList(List<IBlockShape> raw)
        {
            var dic = new Dictionary<int, List<(TwoDPoint, CondAfterCross)>>();
            for (int i = 0; i < raw.Count - 1; i++)
            {
                var blockShape1 = raw[i];
                dic.TryGetValue(i, out var iPtAndCond);
                if (iPtAndCond == null)
                {
                    iPtAndCond = new List<(TwoDPoint, CondAfterCross)>();
                }

                for (int j = i + 1; j < raw.Count; j++)
                {
                    var blockShape2 = raw[j];
                    dic.TryGetValue(j, out var jPtAndCond);
                    if (jPtAndCond == null)
                    {
                        jPtAndCond = new List<(TwoDPoint, CondAfterCross)>();
                    }

                    var crossAnotherBlockShapeReturnCrossPtAndThisCondAnotherCond =
                        blockShape1.CrossAnotherBlockShapeReturnCrossPtAndThisCondAnotherCond(blockShape2);
                    foreach (var (twoDPoint, item2, item3) in crossAnotherBlockShapeReturnCrossPtAndThisCondAnotherCond)
                    {
                        iPtAndCond.Add((twoDPoint, item2));
                        jPtAndCond.Add((twoDPoint, item3));
                    }

                    dic[j] = jPtAndCond;
                }

                dic[i] = iPtAndCond;
            }

            var nowCond = CondAfterCross.MaybeOutToIn;

            var temp = new List<IBlockShape>();

            var res = new List<IBlockShape>();

            for (int i = 0; i < raw.Count; i++)
            {
                dic.TryGetValue(i, out var ptAndCond);
                if (ptAndCond == null)
                {
                    ptAndCond = new List<(TwoDPoint, CondAfterCross)>();
                }

                var blockShape = raw[i];
                var (blockShapes, condAfterCross, list) =
                    blockShape.CutByPointReturnGoodBlockCondAndTemp(nowCond, ptAndCond, temp);
                res.AddRange(blockShapes);
                nowCond = condAfterCross;
                temp = list;
            }

            if (nowCond == CondAfterCross.MaybeOutToIn)
                res.AddRange(temp);

            return res;
        }

        public static QSpace CreateQSpaceByAabbBoxShapes(AabbBoxShape[] aabbBoxShapes, int maxLoadPerQ, Zone zone)
        {
            var joinAabbZone = JoinAabbZone(aabbBoxShapes);
            if (joinAabbZone.IsIn(zone))
            {
                joinAabbZone = zone;
            }

            var qSpace = new QSpaceLeaf(Quad.One, null, joinAabbZone, aabbBoxShapes.ToList());
            return qSpace.TryCovToLimitQSpace(maxLoadPerQ);
        }

        public static QSpace CreateQSpaceByAabbBoxShapes(AabbBoxShape[] aabbBoxShapes, int maxLoadPerQ)
        {
            var joinAabbZone = JoinAabbZone(aabbBoxShapes);
            var qSpace = new QSpaceLeaf(Quad.One, null, joinAabbZone, aabbBoxShapes.ToList());
            return qSpace.TryCovToLimitQSpace(maxLoadPerQ);
        }

        public static List<IBlockShape>? CutShapes(int startN, List<IBlockShape> shapes)
        {
            var shapesCount = shapes.Count;

            if (startN + 1 >= shapesCount)
            {
                return null;
            }


            var list = startN > 0 ? shapes.Take(startN).ToList() : new List<IBlockShape>();

            int mark = startN + 2;
            var shape = shapes[startN];
            var shapeT = shapes[startN + 1];
            for (int i = startN + 1; i < shapesCount; i++)
            {
                var shape1 = shapes[i];
                switch (shape)
                {
                    case ClockwiseTurning clockwiseTurning:
                        switch (shape1)
                        {
                            case ClockwiseTurning clockwiseTurning1:
                                var (item1, item2, item3) = clockwiseTurning.TouchAnotherOne(clockwiseTurning1);
                                if (item1)
                                {
                                    shape = item2;
                                    shapeT = item3;
                                    mark = i + 1;
                                }

                                break;
                            case TwoDVectorLine twoDVectorLine:
                                var (item11, item21, item31) = clockwiseTurning.TouchByLine(twoDVectorLine);
                                if (item11)
                                {
                                    shape = item21;
                                    shapeT = item31;
                                    mark = i + 1;
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case TwoDVectorLine twoDVectorLine:
                        switch (shape1)
                        {
                            case ClockwiseTurning clockwiseTurning1:
                                var (item1, item2, item3) = twoDVectorLine.TouchByCt(clockwiseTurning1);
                                if (item1)
                                {
                                    shape = item2;
                                    shapeT = item3;
                                    mark = i + 1;
                                }

                                break;
                            case TwoDVectorLine twoDVectorLine1:
                                var (item11, item21, item31) = twoDVectorLine.TouchByLineInSamePoly(twoDVectorLine1);
                                if (item11)
                                {
                                    shape = item21;
                                    shapeT = item31;
                                    mark = i + 1;
                                }

                                break;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            list.Add(shape);
            list.Add(shapeT);
            if (mark < shapesCount)
            {
                list.AddRange(shapes.Skip(mark).Take(shapesCount - mark));
            }

            return list;
        }

        public static void LogPt(TwoDPoint? pt)
        {
            if (pt == null)
            {
                Console.Out.WriteLine("pt:::null");
            }
            else
            {
                Console.Out.WriteLine("pt:::" + pt.X + "," + pt.Y);
            }
        }
    }
}