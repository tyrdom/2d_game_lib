using System;
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