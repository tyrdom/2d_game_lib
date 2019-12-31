using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Xsl;

namespace ty_game_lib
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

        public static TwoDPoint? SlideTwoDPoint(List<AabbBoxShape> aabbBoxShapes, AabbBoxShape lineInBoxShape)
        {
            foreach (var aabbBoxShape in aabbBoxShapes)
            {
                var notCross = lineInBoxShape.Zone.NotCross(aabbBoxShape.Zone);
                if (!notCross)
                {
                    switch (lineInBoxShape._shape)
                    {
                        case TwoDVectorLine moveLine:
                            switch (aabbBoxShape._shape)
                            {
                                case ClockwiseTurning blockClockwiseTurning:
                                    var isCross = blockClockwiseTurning.IsCross(moveLine);
                                    if (isCross)
                                    {
                                        var twoDPoint = blockClockwiseTurning.Slide(moveLine.B);
                                        return twoDPoint;
                                    }

                                    break;
                                case TwoDVectorLine blockLine:
                                    var isCrossAnother = blockLine.SimpleIsCross(moveLine);
                                    if (isCrossAnother)
                                    {
                                        var twoDPoint = blockLine.Slide(moveLine.B);
                                        return twoDPoint;
                                    }

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return null;
        }

        public static string ZoneLog(Zone zone)
        {
            return zone.Up + "|" + zone.Down + "|" + zone.Left + "|" + zone.Right;
        }

        public static QSpace CreateQSpaceByAabbBoxShapes(AabbBoxShape[] aabbBoxShapes, int maxLoadPerQ)
        {
            var joinAabbZone = JoinAabbZone(aabbBoxShapes);
            var qSpace = new QSpaceLeaf(Quad.One, joinAabbZone, aabbBoxShapes.ToList());

            return qSpace.TryCovToLimitQSpace(maxLoadPerQ);
        }

        public static void LogPt(TwoDPoint? pt)
        {
            if (pt==null)
            {
                Console.Out.WriteLine("pt:::null");
            }
            else
            {
                Console.Out.WriteLine("pt:::"+pt.X+","+pt.Y);
            }
        }
    }

    public class Either<A, B>
    {
        public A left { get; }
        public B right { get; }

        public Either(A a)
        {
            left = a;
        }

        public Either(B b)
        {
            right = b;
        }

        A Left()
        {
            return left;
        }

        B Right()
        {
            return right;
        }

        object GetValue()
        {
            if (left == null)
            {
                return right;
            }

            {
                return left;
            }
        }
    }
}