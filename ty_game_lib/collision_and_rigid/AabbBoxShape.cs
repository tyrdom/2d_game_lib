using System;
using System.Collections.Generic;
using System.Linq;

namespace collision_and_rigid
{
    public class AabbBoxShape
    {
        public readonly IShape Shape;
        public Zone Zone;


        public AabbBoxShape(Zone zone, IShape shape)
        {
            Zone = zone;
            Shape = shape;
        }

        private bool IsNotTouch(AabbBoxShape another)
        {
            return Zone.NotCross(another.Zone);
        }


        public List<AabbBoxShape> TryTouch(IEnumerable<AabbBoxShape> aabbBoxes)
        {
            return (List<AabbBoxShape>) aabbBoxes.Where(aabbBox => !aabbBox.IsNotTouch(this));
        }


        public Quad? WhichQ(float horizon, float vertical)
        {
            if (Zone.Down >= horizon)
            {
                if (Zone.Left >= vertical) return Quad.One;
                if (Zone.Right <= vertical) return Quad.Two;
            }

            if (!(Zone.Up <= horizon)) return null;
            if (Zone.Left >= vertical) return Quad.Four;
            if (Zone.Right <= vertical) return Quad.Three;

            return null;
        }

        public List<(int, AabbBoxShape)> SplitByQuads(float horizon, float vertical)
        {
            var z1234 = new List<(int, AabbBoxShape)>();
            var z = Zone;
#if DEBUG
            
#endif

            if (z.Down >= horizon)
            {
                if (z.Left >= vertical)
                    z1234.Add((1, this));
//                    z1234[1] = this;
                else if (z.Right <= vertical)
                    z1234.Add((2, this));
                else
                    switch (Shape)
                    {
                        case ClockwiseTurning clockwiseTurning:
//
#if DEBUG
                            Console.Out.WriteLine($"{z.LogSide()}");
                            Console.Out.WriteLine($"{clockwiseTurning.LogPt()}:::{vertical}");
#endif

                            var (lZones, rZones) = clockwiseTurning.CutByV(vertical, z);

//                            Console.Out.WriteLine("L:::" + lZones.Count);
//                            lZones.ForEach(zzz => { Console.Out.WriteLine(SomeTools.ZoneLog(zzz)); });
//
//                            rZones.ForEach(zzz => { Console.Out.WriteLine(SomeTools.ZoneLog(zzz)); });
                            if (lZones != null)
                            {
                                z1234.Add((2, new AabbBoxShape(lZones.Value, Shape)));
                                z1234.Add((1, new AabbBoxShape(rZones!.Value, Shape)));
                            }
                            else
                            {
                                throw new Exception("no good zone");
                            }

                            break;
                        case TwoDVectorLine twoDVectorLine:
                            var (item1, item2) = twoDVectorLine.CutByV(vertical, z);
                            if (item1 != null)
                            {
                                var t1 = (2, new AabbBoxShape(item1.Value, Shape));
                                var t2 = (1, new AabbBoxShape(item2!.Value, Shape));
                                z1234.Add(t1);
                                z1234.Add(t2);
                            }

                            break;
                        default:
                            z1234.Add((1, new AabbBoxShape(new Zone(z.Up, z.Down, vertical, z.Right), Shape)));
                            z1234.Add((2, new AabbBoxShape(new Zone(z.Up, z.Down, z.Left, vertical), Shape)));
                            break;
                    }
            }
            else if (z.Up <= horizon)
            {
                if (z.Left >= vertical)
                    z1234.Add((4, this));
                else if (z.Right <= vertical)
                    z1234.Add((3, this));
                else
                    switch (Shape)
                    {
                        case ClockwiseTurning clockwiseTurning:

                            var (lZones, rZones) = clockwiseTurning.CutByV(vertical, z);
                            if (lZones != null)
                            {
                                z1234.Add((3, new AabbBoxShape(lZones.Value, Shape)));
                                z1234.Add((4, new AabbBoxShape(rZones!.Value, Shape)));
                            }
                            else
                            {
                                throw new Exception("no good zone");
                            }

                            break;
                        case TwoDVectorLine twoDVectorLine:
                            var (item1, item2) = twoDVectorLine.CutByV(vertical, z);
                            if (item1 != null)
                            {
                                var t1 = (3, new AabbBoxShape(item1.Value, Shape));
                                var t2 = (4, new AabbBoxShape(item2!.Value, Shape));
                                z1234.Add(t1);
                                z1234.Add(t2);
                            }

                            break;
                        default:
                            z1234.Add((4, new AabbBoxShape(new Zone(z.Up, z.Down, vertical, z.Right), Shape)));
                            z1234.Add((3, new AabbBoxShape(new Zone(z.Up, z.Down, z.Left, vertical), Shape)));
                            break;
                    }
            }
            else
            {
                if (z.Left >= vertical)
                    switch (Shape)
                    {
                        case ClockwiseTurning clockwiseTurning:

//                            Console.Out.WriteLine(SomeTools.ZoneLog(z));
//                            Console.Out.WriteLine("cutBYH::" + horizon);
                            var (uz, dz) = clockwiseTurning.CutByH(horizon, z);
//                            Console.Out.WriteLine("out::" + SomeTools.ZoneLog(uz.Value) + "AND" +
//                                                  SomeTools.ZoneLog(dz.Value));

                            if (uz != null)
                            {
                                var t1 = (1, new AabbBoxShape(uz.Value, Shape));
                                var t2 = (4, new AabbBoxShape(dz!.Value, Shape));
                                z1234.Add(t1);
                                z1234.Add(t2);
                            }

                            break;
                        case TwoDVectorLine twoDVectorLine:
                            var (uuz, ddz) = twoDVectorLine.CutByH(horizon, z);

                            if (uuz != null)
                            {
                                var t1 = (1, new AabbBoxShape(uuz.Value, Shape));
                                var t2 = (4, new AabbBoxShape(ddz!.Value, Shape));
                                z1234.Add(t1);
                                z1234.Add(t2);
                            }

                            break;
                        default:

                            z1234.Add((1, new AabbBoxShape(new Zone(z.Up, horizon, z.Left, z.Right), Shape)));
                            z1234.Add((4, new AabbBoxShape(new Zone(horizon, z.Down, z.Left, z.Right), Shape)));
                            break;
                    }
                else if (z.Right <= vertical)
                    switch (Shape)
                    {
                        case ClockwiseTurning clockwiseTurning:

//                            Console.Out.WriteLine(SomeTools.ZoneLog(z));
//                            Console.Out.WriteLine("cutBYH::" + horizon);
                            var (uz, dz) = clockwiseTurning.CutByH(horizon, z);
//                            Console.Out.WriteLine("out::" + SomeTools.ZoneLog(uz.Value) + "AND" +
//                                                  SomeTools.ZoneLog(dz.Value));

                            if (uz != null)
                            {
                                var t1 = (2, new AabbBoxShape(uz.Value, Shape));
                                var t2 = (3, new AabbBoxShape(dz!.Value, Shape));
                                z1234.Add(t1);
                                z1234.Add(t2);
                            }

                            break;
                        case TwoDVectorLine twoDVectorLine:
                            var (uuz, ddz) = twoDVectorLine.CutByH(horizon, z);
                            if (uuz != null)
                            {
                                var t1 = (2, new AabbBoxShape(uuz.Value, Shape));
                                var t2 = (3, new AabbBoxShape(ddz!.Value, Shape));
                                z1234.Add(t1);
                                z1234.Add(t2);
                            }

                            break;
                        default:

                            z1234.Add((2, new AabbBoxShape(new Zone(z.Up, horizon, z.Left, z.Right), Shape)));
                            z1234.Add((3, new AabbBoxShape(new Zone(horizon, z.Down, z.Left, z.Right), Shape)));
                            break;
                    }
                else
                    z1234.Add((0, this));
            }


            return z1234;
        }

        public Dictionary<Quad, AabbBoxShape> CutTo4(float horizon, float vertical)
        {
            var z1234 = new Dictionary<Quad, AabbBoxShape>();
            var z = Zone;


            if (z.Down >= horizon)
            {
                if (z.Left >= vertical)
                {
                    z1234[Quad.One] = this;
                }
                else if (z.Right <= vertical)
                {
                    z1234[Quad.Two] = this;
                }
                else
                {
                    z1234[Quad.One] = new AabbBoxShape(new Zone(z.Up, z.Down, vertical, z.Right), Shape);
                    z1234[Quad.Two] = new AabbBoxShape(new Zone(z.Up, z.Down, z.Left, vertical), Shape);
                }
            }
            else if (z.Up <= horizon)
            {
                if (z.Left >= vertical)
                {
                    z1234[Quad.Four] = this;
                }
                else if (z.Right <= vertical)
                {
                    z1234[Quad.Three] = this;
                }
                else
                {
                    z1234[Quad.Four] = new AabbBoxShape(new Zone(z.Up, z.Down, vertical, z.Right), Shape);
                    z1234[Quad.Three] = new AabbBoxShape(new Zone(z.Up, z.Down, z.Left, vertical), Shape);
                }
            }
            else
            {
                if (z.Left >= vertical)
                {
                    z1234[Quad.One] = new AabbBoxShape(new Zone(z.Up, horizon, z.Left, z.Right), Shape);
                    z1234[Quad.Four] = new AabbBoxShape(new Zone(horizon, z.Down, z.Left, z.Right), Shape);
                }
                else if (z.Right <= vertical)
                {
                    z1234[Quad.Two] = new AabbBoxShape(new Zone(z.Up, horizon, z.Left, z.Right), Shape);
                    z1234[Quad.Three] = new AabbBoxShape(new Zone(horizon, z.Down, z.Left, z.Right), Shape);
                }
                else
                {
                    z1234[Quad.One] = new AabbBoxShape(new Zone(z.Up, horizon, vertical, z.Right), Shape);
                    z1234[Quad.Two] = new AabbBoxShape(new Zone(z.Up, horizon, z.Left, vertical), Shape);
                    z1234[Quad.Three] = new AabbBoxShape(new Zone(horizon, z.Down, z.Left, vertical), Shape);
                    z1234[Quad.Four] = new AabbBoxShape(new Zone(horizon, z.Down, vertical, z.Right), Shape);
                }
            }


            return z1234;
        }
    }
}