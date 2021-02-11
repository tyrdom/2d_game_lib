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
#if DEBUG
                Console.Out.WriteLine($"Is Flush {isFlush}");
#endif
                var twoDPoints = pp.Pts.Reverse();
                return new Poly(twoDPoints.ToArray()).ToNotCrossPoly();
            }

            return pp.ToNotCrossPoly();
        }

        public bool CrossAnotherOne(Poly another)
        {
            var twoDVectorLines = CovToLines(true);
            var dVectorLines = another.CovToLines(true);
            var any = twoDVectorLines.Any(x =>
                dVectorLines.Any(y => x.CrossAnotherPointInLinesNotIncludeEnds(y) != null));
            return any;
        }

        public bool IsAwayFrom(Poly another)
        {
            var genZone = another.GenZone();
            var zone = GenZone();
            var notCross = zone.NotCross(genZone);
            if (notCross)
            {
                return true;
            }

            var isIn = genZone.IsIn(zone) || zone.IsIn(genZone);
            if (isIn)
            {
                return false;
            }

            var crossAnotherOne = CrossAnotherOne(another);
            return !crossAnotherOne;
        }

        public bool IsIncludeAnother(Poly another)
        {
            var genZone = GenZone();
            var zone = another.GenZone();
            var isIn = zone.IsIn(genZone);
            var crossAnotherOne = CrossAnotherOne(another);
            var includeAnother = isIn && !crossAnotherOne;
            return includeAnother;
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
                Console.Out.WriteLine($"poly pt::{twoDPoint.ToString()}");
            }
#endif
            var pPts = isBlockIn ? startWithACovNotFlush.Pts : startWithACovNotFlush.Pts.Reverse().ToArray();
            // foreach (var twoDPoint in pPts)
            // {
            //     Console.Out.WriteLine("poly x::" + twoDPoint.X + "    y::" + twoDPoint.Y);
            // }


            var pPtsLength = pPts.Length;

            var fLine = new TwoDVectorLine(pPts[0], pPts[1], true, true);
            var unitF = fLine.GetVector().GetUnit().CounterClockwiseHalfPi().Multi(r);
            var firstBlock = fLine.MoveVector(unitF);
            var nowOnLine = fLine;
            var nowOnBlock = firstBlock;

            for (var i = 1; i < pPtsLength; i++)
            {
                var aPoint = pPts[i];
                var bPoint = pPts[(i + 1) % pPtsLength];

                var line1 = new TwoDVectorLine(aPoint, bPoint, true, true);
                var unitV1 = line1.GetVector().GetUnit().CounterClockwiseHalfPi().Multi(r);
                var fl1 = line1.MoveVector(unitV1);

                var posOf = bPoint.GetPosOf(nowOnLine);
                switch (posOf)
                {
                    case Pt2LinePos.Right:
                        shapes.Add(nowOnBlock);
                        var angle = new ClockwiseBalanceAngle(nowOnBlock.B, aPoint, fl1.A);
                        var clockwiseTurning = new ClockwiseTurning(angle, r, nowOnBlock, fl1);
                        shapes.Add(clockwiseTurning);
                        nowOnBlock = fl1;
                        nowOnLine = line1;
                        break;
                    case Pt2LinePos.On:
                        nowOnBlock = new TwoDVectorLine(nowOnBlock.A, fl1.B, true, true);
                        nowOnLine = line1;
                        break;
                    case Pt2LinePos.Left:
                        shapes.Add(nowOnBlock);
                        nowOnBlock = fl1;
                        nowOnLine = line1;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var pt2LinePos = fLine.B.GetPosOf(nowOnLine);
            switch (pt2LinePos)
            {
                case Pt2LinePos.Right:
                    shapes.Add(nowOnBlock);
                    var angle = new ClockwiseBalanceAngle(nowOnBlock.B, fLine.A, firstBlock.A);
                    var clockwiseTurning = new ClockwiseTurning(angle, r, nowOnBlock, firstBlock);
                    shapes.Add(clockwiseTurning);

                    break;
                case Pt2LinePos.On:
                    shapes[0] = new TwoDVectorLine(nowOnBlock.A, firstBlock.B, true, true);
                    break;
                case Pt2LinePos.Left:
                    shapes.Add(nowOnBlock);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var resShapes = SomeTools.CutInSingleShapeList(shapes);
//            Console.Out.WriteLine("?????" + resShapes.Count);

            var genBlockShapes = resShapes.Where(blockShape => !blockShape.IsEmpty()).ToList();


//            Console.Out.WriteLine("><><><>" + genBlockShapes.Count);

            var checkCloseAndFilter = SomeTools.CheckCloseAndFilter(genBlockShapes);

//            Console.Out.WriteLine("<!><!><!>" + checkCloseAndFilter.Count);

            return checkCloseAndFilter;
        }

        public static List<BlockBox> GenBlockAabbBoxShapes(List<IBlockShape> resShapes)
        {
            var aabbBoxShapes = new List<BlockBox>();
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

            var qSpaceByAabbBoxShapes =
                SomeTools.CreateWalkBlockQSpaceByBlockBoxes(genBlockAabbBoxShapes.ToArray(), limit);

            var block = new WalkBlock(isBlockIn, qSpaceByAabbBoxShapes);
            return block;
        }

        public static WalkBlock GenWalkBlockByBlockShapes(int limit, bool isBlockIn, List<IBlockShape> genBlockShapes)
        {
#if DEBUG
            Console.Out.WriteLine($"blk ::{genBlockShapes.Count}");
#endif
            var genBlockAabbBoxShapes = GenBlockAabbBoxShapes(genBlockShapes);

            var qSpaceByAabbBoxShapes =
                SomeTools.CreateWalkBlockQSpaceByBlockBoxes(genBlockAabbBoxShapes.ToArray(), limit);

            var block = new WalkBlock(isBlockIn, qSpaceByAabbBoxShapes);
            return block;
        }
    }
}