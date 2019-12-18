#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ty_game_lib
{
    public class Block
    {
        private float R;
        private TwoDVectorLine[] Lines;
        private TwoDPoint[] RoundsO;


        public Block(float r, TwoDVectorLine[] lines, TwoDPoint[] roundsO)
        {
            R = r;
            Lines = lines;
            RoundsO = roundsO;
        }

        static Block GenByPoly(Poly p, float r)
        {
            var twoDVectorLines = new List<TwoDVectorLine>();
            var dPoints = new List<TwoDPoint>();

            var startWithACovNotFlush = p.startWithACovAndFlush();

            var pPts = startWithACovNotFlush.Pts;
            TwoDPoint? tempPoint = null;
            bool skip = false;
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

                var startP = tempPoint;
                if (tempPoint == null)
                {
                    startP = fl1.A;
                }


                switch (getposOnLine) //todo
                {
                    case Pt2LinePos.Right:

                        twoDVectorLines.Add(new TwoDVectorLine(startP, fl1.B));
                        dPoints.Add(bPoint);
                        tempPoint = null;
                        break;
                    case Pt2LinePos.On:
                        skip = true;
                        twoDVectorLines.Add(new TwoDVectorLine(startP, fl2.B));
                        break;
                    case Pt2LinePos.Left:
                        var crossAnotherPoint = fl1.CrossAnotherPoint(fl2);

                        if (crossAnotherPoint != null)
                        {
                            var twoDVectorLine = new TwoDVectorLine(startP, crossAnotherPoint);
                            twoDVectorLines.Add(twoDVectorLine);
                            tempPoint = crossAnotherPoint;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new Block(r, twoDVectorLines.ToArray(), dPoints.ToArray());
        }
    }
}