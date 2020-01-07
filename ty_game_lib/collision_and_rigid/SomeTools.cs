using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public static TwoDPoint? SlideTwoDPoint(List<AabbBoxShape> aabbBoxShapes, TwoDVectorLine line,bool isPush)
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
                            var twoDPoint = blockClockwiseTurning.Slide(isPush?line.B:line.A);
                            return twoDPoint;
                        }

                        break;
                    case TwoDVectorLine blockLine:
                        var isCrossAnother = blockLine.SimpleIsCross(line);
                        if (isCrossAnother)
                        {
                            var twoDPoint = blockLine.Slide(isPush?line.B:line.A);
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

        public static QSpace CreateQSpaceByAabbBoxShapes(AabbBoxShape[] aabbBoxShapes, int maxLoadPerQ)
        {
            var joinAabbZone = JoinAabbZone(aabbBoxShapes);
            var qSpace = new QSpaceLeaf(Quad.One, joinAabbZone, aabbBoxShapes.ToList());

            return qSpace.TryCovToLimitQSpace(maxLoadPerQ);
        }

        public static List<IShape>? CutShapes(int startN, List<IShape> shapes)
        {
            var shapesCount = shapes.Count;

            if (startN + 1 >= shapesCount)
            {
                return null;
            }


            var list = startN > 0 ? shapes.Take(startN).ToList() : new List<IShape>();

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
                                var (item11, item21, item31) = twoDVectorLine.TouchByLine(twoDVectorLine1);
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