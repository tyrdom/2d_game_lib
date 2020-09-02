using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace collision_and_rigid
{
    public static class SomeTools
    {
        public static IQSpace CreateEmptyRootBranch(Zone zone)
        {
            var (item1, item2) = zone.GetMid();
            var cutTo4 = zone.CutTo4(item1, item2);


            IQSpace qs1 = new QSpaceLeaf(Quad.One, null, cutTo4[0], new HashSet<AabbBoxShape>());
            IQSpace qs2 = new QSpaceLeaf(Quad.Two, null, cutTo4[1], new HashSet<AabbBoxShape>());
            IQSpace qs3 = new QSpaceLeaf(Quad.Three, null, cutTo4[2], new HashSet<AabbBoxShape>());
            IQSpace qs4 = new QSpaceLeaf(Quad.Four, null, cutTo4[3], new HashSet<AabbBoxShape>());
            var qSpaceBranch = new QSpaceBranch(null, null, zone, new HashSet<AabbBoxShape>(
                )
                ,
                qs1,
                qs2,
                qs3,
                qs4
            );
            return qSpaceBranch;
        }

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
            foreach (var i in Enumerable.Range(1, aabbBoxShapes.Length - 1)) foo = foo.Join(aabbBoxShapes[i].Zone);

            return foo;
        }

        public static void LogZones(AabbBoxShape[] aabbBoxShapes)
        {
            var s = "";
            foreach (var aabbBoxShape in aabbBoxShapes)
            {
                var zone = aabbBoxShape.Zone;
                var zoneLog = ZoneLog(zone);
                s += zoneLog + "\n";
            }

            Console.Out.WriteLine("aabb::" + s + "::bbaa");
        }

        public static IEnumerable<IIdPointShape> FilterToGIdPsList<T>(IQSpace qSpace,
            Func<IIdPointShape, T, bool> funcWithIIdPtsShape,
            T t)
        {
            var dicIntToTu = new HashSet<IIdPointShape>();

            void Act(IIdPointShape id, T tt)
            {
                var withIIdPtsShape = funcWithIIdPtsShape(id, tt);

                if (withIIdPtsShape) dicIntToTu.Add(id);
            }

            qSpace.ForeachDoWithOutMove(Act, t);
            return dicIntToTu;
        }

        public static Dictionary<int, TU> MapToDicGidToSthTool<TU, T>(IQSpace qSpaceBranch,
            Func<IIdPointShape, T, TU> funcWithIIdPtsShape,
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

            qSpaceBranch.ForeachDoWithOutMove(Act, t);
            return dicIntToTu;
        }

        public static TwoDPoint? SlideTwoDPoint(IEnumerable<AabbBoxShape> aabbBoxShapes,
            TwoDVectorLine moveLine,
            bool safe)
        {
            foreach (var aabbBoxShape in aabbBoxShapes)
            {
                var notCross = moveLine.GenZone().NotCross(aabbBoxShape.Zone);
// #if DEBUG
//
//                 Console.Out.WriteLine($"{moveLine.Log()} not cross sssssssssssssss shape {notCross}");
// #endif
                if (notCross) continue;
                var moveLineB = moveLine.B;
                switch (aabbBoxShape.Shape)
                {
                    case ClockwiseTurning blockClockwiseTurning:
                        var isCross = blockClockwiseTurning.IsCross(moveLine);
                        if (isCross)
                        {
                            var twoDPoint = blockClockwiseTurning.Slide(moveLineB, safe);
                            return twoDPoint;
                        }

                        break;
                    case TwoDVectorLine blockLine:
                        var isCrossAnother = moveLine.IsGoTrough(blockLine);

                        if (isCrossAnother)
                        {
// #if DEBUG
//                             Console.Out.WriteLine(
//                                 $" {moveLine.Log()}cross {blockLine.Log()} qqqqqqqqqqq shape {isCrossAnother}");
//                             
// #endif
                            var twoDPoint = blockLine.Slide(moveLineB);
                            return twoDPoint;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return null;
        }


        public static (List<T> left, List<T> right ) CutList<T>(List<T> raw, int i)
        {
            return (raw.GetRange(0, i + 1)
                , raw.GetRange(i + 1, raw.Count - i - 1));
        }


        public static (HashSet<AabbBoxShape>, HashSet<AabbBoxShape>) MovePtsReturnInAndOut(
            Dictionary<int, ITwoDTwoP> gidToMove,
            IEnumerable<AabbBoxShape> aabbBoxShapes, Zone zone)
        {
            var inZone = new HashSet<AabbBoxShape>();
            var outZone = new HashSet<AabbBoxShape>();

            foreach (var aabbPackBoxShape in aabbBoxShapes)
            {
                var shape = aabbPackBoxShape.Shape;

                switch (shape)
                {
                    case IIdPointShape idPointShape:
                        var id = idPointShape.GetId();
                        if (gidToMove.TryGetValue(id, out var vector))
                        {
                            var twoDPoint = idPointShape.Move(vector);
                            if (zone.IncludePt(twoDPoint))
                            {
                                inZone.Add(aabbPackBoxShape);
                            }
                            else
                            {
                                outZone.Add(aabbPackBoxShape);
                            }
                        }
                        else
                        {
                            inZone.Add(aabbPackBoxShape);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(shape));
                }
            }

            return (inZone, outZone);
        }

        public static string ZoneLog(Zone zone)
        {
            return zone.Up + "|" + zone.Down + "|" + zone.Left + "|" + zone.Right;
        }


        private static List<IBlockShape> ReFlush(List<IBlockShape> list)
        {
            var blockShape = list.First();
            return list;
        }

        public static bool CheckFlush(IEnumerable<IBlockShape> l1)
        {
            var blk1F = true;
            TwoDPoint edPt1 = null!;
            foreach (var blockShape in l1)
            {
                var twoPp = blockShape.GetStartPt();

                var b = edPt1 == null || edPt1 == twoPp;
                blk1F = blk1F && b;

                var twoDPoint = blockShape.GetEndPt();

                edPt1 = twoDPoint;
            }

            return blk1F;
        }

        private static (List<IBlockShape>, bool) AddTwoBlocks(
            List<IBlockShape> l1,
            bool l1IsBlockIn,
            List<IBlockShape> l2,
            bool l2IsBlockIn)
        {
            if (l2.Count == 0)
            {
                return (l1, l1IsBlockIn);
            }
#if DEBUG
            Console.Out.WriteLine("blk 1");
            var blk1F = true;
            TwoDPoint edPt1 = null!;
            foreach (var blockShape in l1)
            {
                var twoPp = blockShape.GetStartPt();

                var b = edPt1 == null || edPt1 == twoPp;
                blk1F = blk1F && b;

                var twoDPoint = blockShape.GetEndPt();

                edPt1 = twoDPoint;
                Console.Out.WriteLine($"a blk start {twoPp.Log()} end {twoDPoint.Log()} now flush is {b}");
            }

            Console.Out.WriteLine($"blk 1~~~~ is flush {blk1F}");

            Console.Out.WriteLine("blk 2");
            var blk2F = true;
            TwoDPoint edPt2 = null!;
            foreach (var blockShape in l2)
            {
                var twoPp = blockShape.GetStartPt();
                var b = edPt2 == null || edPt2 == twoPp;
                blk2F = blk2F && b;
                var twoDPoint = blockShape.GetEndPt();
                edPt2 = twoDPoint;
                Console.Out.WriteLine($"a blk start {twoPp.Log()} end {twoDPoint.Log()} now flush is {b}");
            }

            Console.Out.WriteLine($"blk 2~~~~ is flush {blk2F}");
#endif
            //record points
            var dicL1 = new Dictionary<int, List<(TwoDPoint, CondAfterCross)>>();
            var dicL2 = new Dictionary<int, List<(TwoDPoint, CondAfterCross)>>();

            for (var i = 0; i < l1.Count; i++)
            {
#if DEBUG
                // Console.Out.WriteLine("s dic::::" + i);
#endif


                var blockShapeFromL1 = l1[i];
                if (!dicL1.TryGetValue(i, out var df1))
                {
                    df1 = new List<(TwoDPoint, CondAfterCross)>();
                }

                for (var j = 0; j < l2.Count; j++)
                {
// #if DEBUG
//                     Console.Out.WriteLine("l1::"+i+"  l2::"+j);
// #endif
                    var blockShapeFromL2 = l2[j];
                    var crossAnotherBlockShapeReturnIsChangeAndBlocks =
                        blockShapeFromL1.CrossAnotherBlockShapeReturnCrossPtAndThisCondAnotherCond(blockShapeFromL2);


                    if (!dicL2.TryGetValue(j, out var df2)) df2 = new List<(TwoDPoint, CondAfterCross)>();

                    if (crossAnotherBlockShapeReturnIsChangeAndBlocks.Count > 0)
                    {
//                        Console.Out.WriteLine("!@#$!@#$");
                        foreach (var (twoDPoint, item2, item3) in crossAnotherBlockShapeReturnIsChangeAndBlocks)
                        {
                            if (df1 != null)
                                df1.Add((twoDPoint, item2));
                            else
                                df1 = new List<(TwoDPoint, CondAfterCross)> {(twoDPoint, item2)};

                            if (df2 != null)
                                df2.Add((twoDPoint, item3));
                            else
                                df2 = new List<(TwoDPoint, CondAfterCross)> {(twoDPoint, item3)};
                        }

                        dicL2[j] = df2;
                    }

                    if (df1 != null) dicL1[i] = df1;
                }
            }
#if DEBUG
            Console.Out.WriteLine("In dic1::\n" + dicL1.Count);
            foreach (var keyValuePair in dicL1)
            {
                Console.Out.WriteLine("key::" + keyValuePair.Key + $" {l1[keyValuePair.Key].Log()}  Count::" +
                                      (keyValuePair.Value?.Count ?? 0));
            }

            Console.Out.WriteLine("In dic2::\n" + dicL2.Count);
            foreach (var keyValuePair in dicL2)
            {
                Console.Out.WriteLine("key::" + keyValuePair.Key + $" {l2[keyValuePair.Key].Log()}  Count::" +
                                      (keyValuePair.Value?.Count ?? 0));
            }
#endif
            var resL1 = new List<IBlockShape>();
            var resL2 = new List<IBlockShape>();

            var l1QSpace = CreateQSpaceByAabbBoxShapes(Poly.GenBlockAabbBoxShapes(l1).ToArray(), 100);

            var block1 = new WalkBlock(l1IsBlockIn, l1QSpace);
            var l2QSpace = CreateQSpaceByAabbBoxShapes(Poly.GenBlockAabbBoxShapes(l2).ToArray(), 100);

            var block2 = new WalkBlock(l2IsBlockIn, l2QSpace);

            var temp1 = new List<IBlockShape>();
            var temp2 = new List<IBlockShape>();
            for (var i = 0; i < l1.Count; i++)
            {
                var blockShape = l1[i];

                dicL1.TryGetValue(i, out var ptsAndCond);
                ptsAndCond ??= new List<(TwoDPoint, CondAfterCross)>();
                var startPt = blockShape.GetStartPt();
                var b1 = block2.RealCoverPoint(startPt);

                var startCond = b1 ? CondAfterCross.ToIn : CondAfterCross.ToOut;
                var endPt = blockShape.GetEndPt();
                var b = block2.RealCoverPoint(endPt);
                var endCond = b ? CondAfterCross.ToIn : CondAfterCross.ToOut;
#if DEBUG
                Console.Out.WriteLine($"{blockShape.Log()}is startPt in {b1} end in {b}");
#endif
                var (blockShapes, _, item3) =
                    blockShape.CutByPointReturnGoodBlockCondAndTemp(startCond, ptsAndCond, temp1, endCond);
                resL1.AddRange(blockShapes);
                temp1 = item3;
            }


            for (var i = 0; i < l2.Count; i++)
            {
                var blockShape = l2[i];

                dicL2.TryGetValue(i, out var ptsAndCond2);
                ptsAndCond2 ??= new List<(TwoDPoint, CondAfterCross)>();
                var startPt = blockShape.GetStartPt();
                var b1 = block1.RealCoverPoint(startPt);
                var startCond = b1 ? CondAfterCross.ToIn : CondAfterCross.ToOut;

                var endPt = blockShape.GetEndPt();

                var b = block1.RealCoverPoint(endPt);
                var cond = b ? CondAfterCross.ToIn : CondAfterCross.ToOut;
#if DEBUG
                Console.Out.WriteLine($"stPt {startPt.Log()} is {startCond} edPt {endPt.Log()} is {cond}");
#endif
                var (blockShapes, condAfterCross, item3) =
                    blockShape.CutByPointReturnGoodBlockCondAndTemp(startCond, ptsAndCond2, temp2, cond);
                resL2.AddRange(blockShapes);
                temp2 = item3;
            }

            resL1.AddRange(resL2);

            return (resL1.Where(x => !x.IsEmpty()).ToList(), l1IsBlockIn && l2IsBlockIn);
        }


        public static List<IBlockShape> CutInSingleShapeList(List<IBlockShape> raw)
        {
            var dic = new Dictionary<int, List<(TwoDPoint, CondAfterCross)>>();
            for (var i = 0; i < raw.Count - 1; i++)
            {
                var blockShape1 = raw[i];
                dic.TryGetValue(i, out var iPtAndCond);
                if (iPtAndCond == null) iPtAndCond = new List<(TwoDPoint, CondAfterCross)>();

                for (var j = i + 1; j < raw.Count; j++)
                {
                    var blockShape2 = raw[j];
                    if (j == i + 1)
                        if (blockShape1.GetType() != blockShape2.GetType())
                            continue;

                    dic.TryGetValue(j, out var jPtAndCond);
                    if (jPtAndCond == null) jPtAndCond = new List<(TwoDPoint, CondAfterCross)>();

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

//            foreach (KeyValuePair<int, List<(TwoDPoint, CondAfterCross)>> keyValuePair in dic)
//            {
//                Console.Out.WriteLine("dic key::::" + keyValuePair.Key + ":::Count:::" + keyValuePair.Value.Count);
//                foreach (var valueTuple in keyValuePair.Value)
//                {
//                    Console.Out.WriteLine("pt::" + valueTuple.Item1.X + '|' + valueTuple.Item1.Y + "  Sigh:::" +
//                                          valueTuple.Item2);
//                }
//            }

            var nowCond = CondAfterCross.MaybeOutToIn;

            var temp = new List<IBlockShape>();

            var res = new List<IBlockShape>();

            for (var i = 0; i < raw.Count; i++)
            {
//                Console.Out.WriteLine("now on block````" + i);
                dic.TryGetValue(i, out var ptAndCond);
                if (ptAndCond == null) ptAndCond = new List<(TwoDPoint, CondAfterCross)>();

                var blockShape = raw[i];
                var (blockShapes, condAfterCross, list) =
                    blockShape.CutByPointReturnGoodBlockCondAndTemp(nowCond, ptAndCond, temp,
                        CondAfterCross.MaybeOutToIn);
                res.AddRange(blockShapes);
                nowCond = condAfterCross;
                temp = list;
//                Console.Out.WriteLine("res::" + res.Count);
//                foreach (var shape in res)
//                {
//                    var startPtPoint = shape.GetStartPt();
//                    var ePoint = shape.GetEndPt();
//
//                    Console.Out.WriteLine("resBlockStart::" + startPtPoint.X + '|' + startPtPoint.Y + "  end:::" +
//                                          ePoint.X + '|' + ePoint.Y);
//                }
//
//                Console.Out.WriteLine("temp::" + temp.Count);
//                foreach (var shape in temp)
//                {
//                    var startPtPoint = shape.GetStartPt();
//                    var ePoint = shape.GetEndPt();
//
//                    Console.Out.WriteLine("tempBlockStart::" + startPtPoint.X + '|' + startPtPoint.Y + "  end:::" +
//                                          ePoint.X + '|' + ePoint.Y);
//                }
            }

            res.AddRange(temp);

            return res;
        }

        private static List<(Poly, bool)> CenterPolys(List<(Poly, bool)> raw)
        {
            var zones = raw.Select(tuple => tuple.Item1.GenZone()).ToList();
            var yMax = zones.Select(zone => zone.Up).Max();
            var yMin = zones.Select(zone => zone.Down).Min();
            var xMax = zones.Select(zone => zone.Right).Max();
            var xMin = zones.Select(zone => zone.Left).Min();
            var cPoint = new TwoDPoint((xMax + xMin) / 2, (yMax + yMin) / 2);
            var mVector = cPoint.GenVector(new TwoDPoint(0, 0));
            return raw.Select(poly =>
                (poly.Item1.Move(mVector), poly.Item2)
            ).ToList();
        }


        public static WalkBlock GenWalkBlockByPolys(List<(Poly, bool)> rawData, float r, int limit)
        {
            var (firstPoly, item2) = rawData[0];

#if DEBUG

#endif

            var genBlockShapes = (firstPoly.GenBlockShapes(r, item2), item2);

            for (var i = 1; i < rawData.Count; i++)
            {
                var (poly, b) = rawData[i];
                var blockShapes = poly.GenBlockShapes(r, b);
                genBlockShapes = AddTwoBlocks(genBlockShapes.Item1, genBlockShapes.item2,
                    blockShapes, b);
            }

            var genWalkBlockByBlockShapes =
                Poly.GenWalkBlockByBlockShapes(limit, genBlockShapes.item2, genBlockShapes.Item1);

            return genWalkBlockByBlockShapes;
        }

        public static HashSet<TX> ListToHashSet<TX>(IEnumerable<TX> aList)
        {
#if NETCOREAPP3
            var  hashSet = aList.ToHashSet();
#else
            var hashSet = new HashSet<TX>();
            foreach (var xe in aList)
            {
                hashSet.Add(xe);
            }
#endif
            return hashSet;
        }

        public static IQSpace CreateQSpaceByAabbBoxShapes(AabbBoxShape[] aabbBoxShapes, int maxLoadPerQ, Zone zone)
        {
            var joinAabbZone = JoinAabbZone(aabbBoxShapes);
            if (joinAabbZone.IsIn(zone)) joinAabbZone = zone;

            var aabbPackPackBoxShapes = ListToHashSet(aabbBoxShapes);


            var qSpace = new QSpaceLeaf(Quad.One, null, joinAabbZone, aabbPackPackBoxShapes);
            return qSpace.TryCovToLimitQSpace(maxLoadPerQ);
        }

        public static IQSpace CreateQSpaceByAabbBoxShapes(AabbBoxShape[] aabbBoxShapes, int maxLoadPerQ)
        {
            var joinAabbZone = JoinAabbZone(aabbBoxShapes);

            var aabbPackPackBoxShapes = ListToHashSet(aabbBoxShapes);


            var qSpace = new QSpaceLeaf(Quad.One, null, joinAabbZone, aabbPackPackBoxShapes);
#if DEBUG
            Console.Out.WriteLine($"aabb num::{qSpace.AabbPackBoxShapes.Count}");
#endif
            return qSpace.TryCovToLimitQSpace(maxLoadPerQ);
        }

        public static void LogPt(TwoDPoint? pt)
        {
            if (pt == null)
                Console.Out.WriteLine("pt:::null");
            else
                Console.Out.WriteLine("pt:::" + pt.X + "," + pt.Y);
        }

        public static List<IBlockShape> CheckCloseAndFilter(List<IBlockShape> blockShapes)
        {
            var res = new List<IBlockShape>();
            var beforeOk = new List<int>();
            var afterOk = new List<int>();
            for (var i = 0; i < blockShapes.Count; i++)
            {
                var shapeI = blockShapes[i];

                for (var j = i + 1; j < blockShapes.Count; j++)
                {
                    var shapeJ = blockShapes[j];

                    var checkAfter = shapeI.CheckAfter(shapeJ);
                    var checkBefore = shapeI.CheckBefore(shapeJ);
                    if (checkAfter)
                    {
                        afterOk.Add(i);
                        beforeOk.Add(j);
                    }

                    if (checkBefore)
                    {
                        beforeOk.Add(i);
                        afterOk.Add(j);
                    }
                }
            }

            return blockShapes.Where((shape, i) => afterOk.Contains(i) && beforeOk.Contains(i)).ToList();
        }
    }
}