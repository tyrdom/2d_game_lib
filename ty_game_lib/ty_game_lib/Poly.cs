using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Xml.Schema;


namespace ty_game_lib
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
            var pt2LinePos = Pts[o].GetposOnLine(twoDVectorLine1);
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

        public Block GenByPoly(float r, int limit)
        {
            var shapes = new List<IShape>();

            var startWithACovNotFlush = StartWithACovAndFlush();
//            startWithACovNotFlush.ShowPts();
            var pPts = startWithACovNotFlush.Pts;
            TwoDPoint? tempPoint = null;
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

                var getposOnLine = cPoint.GetposOnLine(line1);

                var startP = tempPoint ?? fl1.A;
//                Console.Out.WriteLine(startP.X + "|" + startP.Y);
                switch (getposOnLine)
                {
                    case Pt2LinePos.Right:

                        shapes.Add(new TwoDVectorLine(startP, fl1.B));

                        var angle = new ClockwiseBalanceAngle(fl1.B, bPoint, fl2.A);
                        var either = new ClockwiseTurning(angle, r, fl1, fl2);
                        shapes.Add(either);
                        tempPoint = null;
                        break;
                    case Pt2LinePos.On:
                        skip = true;
                        shapes.Add(new TwoDVectorLine(startP, fl2.B));
                        break;
                    case Pt2LinePos.Left:
                        var crossAnotherPoint = fl1.CrossAnotherPoint(fl2);

                        if (crossAnotherPoint != null)
                        {
                            var twoDVectorLine = new TwoDVectorLine(startP, crossAnotherPoint);
                            shapes.Add(twoDVectorLine);
                            tempPoint = crossAnotherPoint;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var aabbBoxShapes = new List<AabbBoxShape>();

            var shapesCount = shapes.Count;
            foreach (var i in Enumerable.Range(0, shapesCount))
            {
                var shape = shapes[i];
                switch (shape)
                {
                    case ClockwiseTurning clockwiseTurning:

                        var last = shapes[(i - 1) % shapesCount];
                        var next = shapes[(i + 1) % shapesCount];


                        var a = last switch
                        {
                            TwoDVectorLine twoDVectorLine => twoDVectorLine,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        var b = next switch
                        {
                            TwoDVectorLine t => t,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        var covToAabbPackBoxes = new ClockwiseTurning(clockwiseTurning.AOB, r, a, b)
                            .CovToVertAabbPackBoxes();
                        aabbBoxShapes.AddRange(covToAabbPackBoxes);
                        break;
                    case TwoDVectorLine twoDVectorLine:
                        var covToAabbPackBox = twoDVectorLine.CovToAabbPackBox();
                        aabbBoxShapes.Add(covToAabbPackBox);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(shape));
                }
            }

            var boxShapes = aabbBoxShapes.ToArray();
//            SomeTools.LogZones(boxShapes);

            var qSpaceByAabbBoxShapes = SomeTools.CreateQSpaceByAabbBoxShapes(boxShapes, limit);


            return new Block(r, qSpaceByAabbBoxShapes);
        }
    }
}