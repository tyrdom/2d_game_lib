using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Xml.Schema;


namespace collision_and_rigid
{
    public class Poly
    {
        public Poly(TwoDPoint[] pts)
        {
            if (pts.Length >= 3)

            {
                Pts = pts;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public TwoDPoint[] Pts { get; }


        void ShowPts()
        {
            foreach (var twoDPoint in Pts)
            {
                Console.Out.WriteLine("pt:" + twoDPoint.X + "|" + twoDPoint.Y);
            }
        }

        public Poly ToNotCrossPoly()
        {
            var twoDPoints = Pts;
            var length = twoDPoints.Length;
            foreach (var i in Enumerable.Range(0, length - 1))
            {
                var twoDVectorLineA = new TwoDVectorLine(twoDPoints[i], twoDPoints[(i + 1)]);
                foreach (var j in Enumerable.Range(i + 2, length - i - 2))
                {
                    var twoDVectorLineB = new TwoDVectorLine(twoDPoints[j], twoDPoints[(j + 1) % length]);
                    if (twoDVectorLineA.IsCrossAnother(twoDVectorLineB))
                    {
                        twoDPoints = SomeTools.SwapPoints(twoDPoints, i + 1, j);
                    }
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
            var m = (n - 1) % ptsLength;
            var o = (n + 1) % ptsLength;
            var twoDVectorLine1 = new TwoDVectorLine(Pts[m], Pts[n]);
            var pt2LinePos = Pts[o].game_stuff(twoDVectorLine1);
            return pt2LinePos switch
            {
                Pt2LinePos.Right => true,
                Pt2LinePos.On => throw new ArgumentOutOfRangeException(),
                Pt2LinePos.Left => false,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public Poly StartWithACovAndFlush()
        {
            Poly poly;
            if (!IsFlush())
            {
                var twoDPoints = Pts.Reverse();
                poly = new Poly(twoDPoints.ToArray()).ToNotCrossPoly();
            }
            else
            {
                poly = ToNotCrossPoly();
            }

            var n = poly.GetACovPointNum();
            var ptsLength = Pts.Length;

            return new Poly(Enumerable.Range(0, ptsLength).Select(i => (n + i) % ptsLength).Select(p => Pts[p])
                .ToArray());
        }

        public List<TwoDVectorLine> CovToLines()
        {
            var startWithACovAndFlush = StartWithACovAndFlush();
            var pts = startWithACovAndFlush.Pts;
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

        public WalkBlock GenWalkBlockByPoly(float r, int limit)
        {
            var shapes = new List<IShape>();

            var startWithACovNotFlush = StartWithACovAndFlush();
//            startWithACovNotFlush.ShowPts();
            var pPts = startWithACovNotFlush.Pts;

            var skip = false;
            var pPtsLength = pPts.Length;
            foreach (var i in Enumerable.Range(0, pPtsLength))
            {
//                Console.Out.WriteLine(i);
                if (skip)
                {
                    skip = false;
                    continue;
                }

                var aPoint = pPts[i];
//                Console.Out.WriteLine("apx:" + aPoint.X + "!" + aPoint.Y);
                var bPoint = pPts[(i + 1) % pPtsLength];
                var cPoint = pPts[(i + 2) % pPtsLength];

                var line1 = new TwoDVectorLine(aPoint, bPoint);
                var line2 = new TwoDVectorLine(bPoint, cPoint);
                var unitV1 = line1.GetVector().GetUnit().CounterClockwiseHalfPi().Multi(r);
                var unitV2 = line2.GetVector().GetUnit().CounterClockwiseHalfPi().Multi(r);

                var fl1 = line1.MoveVector(unitV1);

                var fl2 = line2.MoveVector(unitV2);

                var getposOnLine = cPoint.game_stuff(line1);


                switch (getposOnLine)
                {
                    case Pt2LinePos.Right:

                        shapes.Add(fl1);

                        var angle = new ClockwiseBalanceAngle(fl1.B, bPoint, fl2.A);
                        var clockwiseTurning = new ClockwiseTurning(angle, r, fl1, fl2);
                        shapes.Add(clockwiseTurning);

                        break;
                    case Pt2LinePos.On:
                        skip = true;
                        shapes.Add(new TwoDVectorLine(fl1.A, fl2.B));
                        break;
                    case Pt2LinePos.Left:

                        shapes.Add(fl1);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }


            var shapesCount = shapes.Count;

            var resShapes = shapes;
            foreach (var i in Enumerable.Range(0, shapesCount))
            {
                var cutShapes = SomeTools.CutShapes(i, resShapes);
                if (cutShapes != null)
                {
                    resShapes = cutShapes;
                }
                else
                {
                    break;
                }
            }

            var aabbBoxShapes = new List<AabbBoxShape>();
            foreach (var shape in resShapes)
            {
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
            }

            var qSpaceByAabbBoxShapes = SomeTools.CreateQSpaceByAabbBoxShapes(aabbBoxShapes.ToArray(), limit);

            var block = new WalkBlock(r, qSpaceByAabbBoxShapes);
            return block;
        }
    }
}