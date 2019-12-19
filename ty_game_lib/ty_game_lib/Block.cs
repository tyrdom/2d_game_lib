#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;


namespace ty_game_lib
{
    public class Block : Shape
    {
        private float R;
        private AabbPackBox[] BlockElements;


        public Block(float r, AabbPackBox[] blockElements)
        {
            R = r;
            BlockElements = blockElements;
        }

        static Block GenByPoly(Poly p, float r)
        {
            var twoDVectorLines = new List<AabbPackBox>();


            var startWithACovNotFlush = p.startWithACovAndFlush();

            var pPts = startWithACovNotFlush.Pts;
            TwoDPoint? tempPoint = null;
            var skip = false;
            var pPtsLength = pPts.Length;
            foreach (var i in Enumerable.Range(0, pPtsLength))
            {
                if (skip)
                {
                    skip = false;
                    continue;
                }

                var aPoint = pPts[i];
                var bPoint = pPts[(i + 1) % pPtsLength];
                var cPoint = pPts[(i + 2) % pPtsLength];

                var line1 = new TwoDVectorLine(aPoint, bPoint);
                var line2 = new TwoDVectorLine(bPoint, bPoint);
                var unitV1 = line1.GetVector().GetUnit().DicHalfPi().Multi(r);
                var unitV2 = line2.GetVector().GetUnit().DicHalfPi().Multi(r);

                var fl1 = line1.MoveVector(unitV1);

                var fl2 = line2.MoveVector(unitV2);

                var getposOnLine = cPoint.GetposOnLine(line1);

                var startP = tempPoint ?? fl1.A;

                switch (getposOnLine)
                {
                    case Pt2LinePos.Right:

                        twoDVectorLines.Add(new TwoDVectorLine(startP, fl1.B).CovToAabbPackBox());

                        var angle = new ClockwiseAngle(fl1.B, bPoint, fl2.A);
                        var either = new ClockwiseSector(angle, r);
                        twoDVectorLines.Add(either.CovToAabbPackBox());
                        tempPoint = null;
                        break;
                    case Pt2LinePos.On:
                        skip = true;
                        twoDVectorLines.Add(new TwoDVectorLine(startP, fl2.B).CovToAabbPackBox());
                        break;
                    case Pt2LinePos.Left:
                        var crossAnotherPoint = fl1.CrossAnotherPoint(fl2);

                        if (crossAnotherPoint != null)
                        {
                            var twoDVectorLine = new TwoDVectorLine(startP, crossAnotherPoint);
                            twoDVectorLines.Add(twoDVectorLine.CovToAabbPackBox());
                            tempPoint = crossAnotherPoint;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new Block(r, twoDVectorLines.ToArray());
        }

        public AabbPackBox CovToAabbPackBox()
        {
            var foo = BlockElements[0].Zone;
            foreach (var i in Enumerable.Range(1, BlockElements.Length))
            {
                foo = foo.Join(BlockElements[i].Zone);
            }

            return new AabbPackBox(foo, this);
        }
    }
}