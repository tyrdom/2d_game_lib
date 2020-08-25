using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace collision_and_rigid
{
    public class Poly
    {
        public Poly(TwoDPoint[] pts)
        {
            if (pts.Length >= 3 && !CheckCross(pts) && CheckNoSame(pts))
                Pts = pts;
            else
                throw new ArgumentOutOfRangeException();
        }

        public TwoDPoint[] Pts { get; }

        public Poly Move(TwoDVector mv)
        {
            var twoDPoints = Pts.Select(point => point.Move(mv)).ToArray();
            var poly = new Poly(twoDPoints);
            return poly;
        }

        public Zone GenZone()
        {
            var xMax = Pts.Select(x => x.X).Max();
            var xMin = Pts.Select(x => x.X).Min();
            var yMax = Pts.Select(x => x.Y).Max();
            var yMin = Pts.Select(x => x.Y).Min();

            return new Zone(yMax, yMin, xMin, xMax);
        }

        private void ShowPts()
        {
            foreach (var twoDPoint in Pts) Console.Out.WriteLine("pt:" + twoDPoint.X + "|" + twoDPoint.Y);
        }

        public Poly ClockTurnAboutZero(TwoDVector aim)
        {
            var enumerable = Pts.Select(twoDPoint => twoDPoint.ClockTurnAboutZero(aim)).ToArray();
            return new Poly(enumerable);
        }

        private static bool CheckNoSame(TwoDPoint[] pts)
        {
            for (var i = 0; i < pts.Length; i++)
            for (var j = i + 1; j < pts.Length - j; j++)
            {
                var b = pts[i].Same(pts[j]);
                if (b) return false;
            }

            return true;
        }

        private static bool CheckCross(TwoDPoint[] pts)
        {
            for (var i = 0; i < pts.Length - 1; i++)
            {
                var twoDVectorLineA = new TwoDVectorLine(pts[i], pts[i + 1]);
                for (var j = i + 2; j < pts.Length - j; j++)
                {
                    var twoDVectorLineB = new TwoDVectorLine(pts[j], pts[(j + 1) % pts.Length]);
                    if (twoDVectorLineA.IsCrossAnother(twoDVectorLineB)) return true;
                }
            }

            return false;
        }

        public Poly ToNotCrossPoly()
        {
            var twoDPoints = Pts;
            var length = twoDPoints.Length;
            foreach (var i in Enumerable.Range(0, length - 1))
            {
                var twoDVectorLineA = new TwoDVectorLine(twoDPoints[i], twoDPoints[i + 1]);
                foreach (var j in Enumerable.Range(i + 2, length - i - 2))
                {
                    var twoDVectorLineB = new TwoDVectorLine(twoDPoints[j], twoDPoints[(j + 1) % length]);
                    if (twoDVectorLineA.IsCrossAnother(twoDVectorLineB))
                        twoDPoints = SomeTools.SwapPoints(twoDPoints, i + 1, j);
                }
            }

            return new Poly(twoDPoints);
        }


        public int GetACovPointNum()
        {
            var n = 0;
            var pt = Pts[0];
            var f = pt.X;
            foreach (var i in Enumerable.Range(0, Pts.Length))
            {
                var x = Pts[i].X;
                if (x < f) continue;
                n = i;
                f = x;
            }


            return n;
        }

        public bool IsFlush()
        {
            var n = GetACovPointNum();

            var ptsLength = Pts.Length;
            var m = (n + ptsLength - 1) % ptsLength;
            var o = (n + 1) % ptsLength;
            var twoDVectorLine1 = new TwoDVectorLine(Pts[m], Pts[n]);
            var pt2LinePos = Pts[o].GetPosOf(twoDVectorLine1);
            return pt2LinePos switch
            {
                Pt2LinePos.Right => true,
                Pt2LinePos.On => throw new ArgumentOutOfRangeException(),
                Pt2LinePos.Left => false,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public Poly StartWithACovAndClockwise()
        {
            var n = GetACovPointNum();
            var ptsLength = Pts.Length;

            var dPoints = Enumerable.Range(0, ptsLength).Select(i => (n + i) % ptsLength).Select(p => Pts[p])
                .ToArray();
            var pp = new Poly(dPoints);

            var isFlush = pp.IsFlush();

            if (!isFlush)
            {
                Console.Out.WriteLine($"Is Flush {isFlush}");
                var twoDPoints = pp.Pts.Reverse();
                return new Poly(twoDPoints.ToArray()).ToNotCrossPoly();
            }

            return pp.ToNotCrossPoly();
        }


        public List<TwoDVectorLine> CovToLines(bool isBlockIn)
        {
            var startWithACovAndFlush = StartWithACovAndClockwise();
// #if DEBUG
//             Console.Out.WriteLine("poly ::");
//             foreach (var twoDPoint in startWithACovAndFlush.Pts)
//             {
//                 Console.Out.WriteLine($"poly pt::{twoDPoint.Log()}");
//             }
// #endif
            var pts = isBlockIn ? startWithACovAndFlush.Pts : startWithACovAndFlush.Pts.Reverse().ToArray();


            var aabbBoxShapes = new List<TwoDVectorLine>();

            for (var i = 0; i < pts.Length - 1; i++)
            {
                var a = pts[i];
                var bPoint = pts[i + 1];
                var line = new TwoDVectorLine(a, bPoint);
                aabbBoxShapes.Add(line);
            }

            return aabbBoxShapes;
        }

        public List<IBlockShape> GenBlockShapes(float r, bool isBlockIn)
        {
            var shapes = new List<IBlockShape>();

            var startWithACovNotFlush = StartWithACovAndClockwise();
#if DEBUG
            Console.Out.WriteLine("poly ::~~~");
            foreach (var twoDPoint in startWithACovNotFlush.Pts)
            {
                Console.Out.WriteLine($"poly pt::{twoDPoint.Log()}");
            }
#endif
            var pPts = isBlockIn ? startWithACovNotFlush.Pts : startWithACovNotFlush.Pts.Reverse().ToArray();
            // foreach (var twoDPoint in pPts)
            // {
            //     Console.Out.WriteLine("poly x::" + twoDPoint.X + "    y::" + twoDPoint.Y);
            // }

            var skip = false;
            var pPtsLength = pPts.Length;
            for (var i = 0; i < pPtsLength; i++)
            {
                if (skip)
                {
                    skip = false;
                    continue;
                }

                var aPoint = pPts[i];
//                Console.Out.WriteLine("apx:" + aPoint.X + "!" + aPoint.Y);
                var bPoint = pPts[(i + 1) % pPtsLength];
                var cPoint = pPts[(i + 2) % pPtsLength];

                var line1 = new TwoDVectorLine(aPoint, bPoint, true, true);
                var line2 = new TwoDVectorLine(bPoint, cPoint, true, true);
                var unitV1 = line1.GetVector().GetUnit().CounterClockwiseHalfPi().Multi(r);
                var unitV2 = line2.GetVector().GetUnit().CounterClockwiseHalfPi().Multi(r);

                var fl1 = line1.MoveVector(unitV1);

                var fl2 = line2.MoveVector(unitV2);

                var posOf = cPoint.GetPosOf(line1);
                switch (posOf)
                {
                    case Pt2LinePos.Right:
//                            Console.Out.WriteLine("fl1:::" + fl1.A.X + "|" + fl1.A.Y + "----"+ fl1.B.X + '|' + fl1.B.Y);
                        shapes.Add(fl1);
                        var angle = new ClockwiseBalanceAngle(fl1.B, bPoint, fl2.A);
                        var clockwiseTurning = new ClockwiseTurning(angle, r, fl1, fl2);
                        shapes.Add(clockwiseTurning);
                        break;
                    case Pt2LinePos.On:
                        skip = true;
                        shapes.Add(new TwoDVectorLine(fl1.A, fl2.B, true, true));
                        break;
                    case Pt2LinePos.Left:
                        shapes.Add(fl1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

//
//            Console.Out.WriteLine("!!!!!" + shapes.Count);
//            foreach (var blockShape in shapes)
//            {
//                var twoDPoint = blockShape.GetStartPt();
//                var dPoint = blockShape.GetEndPt();
//                Console.Out.WriteLine("start::" + twoDPoint.X + '|' + twoDPoint.Y + "  end::" + dPoint.X + '|' +
//                                      dPoint.Y);
//            }

            var resShapes = SomeTools.CutInSingleShapeList(shapes);
//            Console.Out.WriteLine("?????" + resShapes.Count);

            var genBlockShapes = resShapes.Where(blockShape => !blockShape.IsEmpty()).ToList();


//            Console.Out.WriteLine("><><><>" + genBlockShapes.Count);

            var checkCloseAndFilter = SomeTools.CheckCloseAndFilter(genBlockShapes);

//            Console.Out.WriteLine("<!><!><!>" + checkCloseAndFilter.Count);

            return checkCloseAndFilter;
        }

        public static List<AabbBoxShape> GenBlockAabbBoxShapes(List<IBlockShape> resShapes)
        {
            var aabbBoxShapes = new List<AabbBoxShape>();
            foreach (var shape in resShapes)
                switch (shape)
                {
                    case ClockwiseTurning clockwiseTurning:
                        var covToVertAabbPackBoxes = clockwiseTurning.CovToVertAabbPackBoxes();
                        aabbBoxShapes.AddRange(covToVertAabbPackBoxes);
                        break;
                    case TwoDVectorLine twoDVectorLine:
                        var toAabbPackBox = twoDVectorLine.CovToAabbPackBox();
                        aabbBoxShapes.Add(toAabbPackBox);
                        break;
                }

            return aabbBoxShapes;
        }

        public WalkBlock GenWalkBlockByPoly(float r, int limit, bool isBlockIn)
        {
            var genBlockShapes = GenBlockShapes(r, isBlockIn);
            if (genBlockShapes.Count <= 1) return new WalkBlock(true, null);

            var genBlockAabbBoxShapes = GenBlockAabbBoxShapes(genBlockShapes);

            var qSpaceByAabbBoxShapes = SomeTools.CreateQSpaceByAabbBoxShapes(genBlockAabbBoxShapes.ToArray(), limit);

            var block = new WalkBlock(isBlockIn, qSpaceByAabbBoxShapes);
            return block;
        }

        public static WalkBlock GenWalkBlockByBlockShapes(int limit, bool isBlockIn, List<IBlockShape> genBlockShapes)
        {
#if DEBUG
            Console.Out.WriteLine($"blk ::{genBlockShapes.Count}");
#endif
            var genBlockAabbBoxShapes = GenBlockAabbBoxShapes(genBlockShapes);

            var qSpaceByAabbBoxShapes = SomeTools.CreateQSpaceByAabbBoxShapes(genBlockAabbBoxShapes.ToArray(), limit);

            var block = new WalkBlock(isBlockIn, qSpaceByAabbBoxShapes);
            return block;
        }
    }
}