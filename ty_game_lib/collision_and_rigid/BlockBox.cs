using System;
using System.Collections.Generic;
using System.Linq;

namespace collision_and_rigid
{
    public class BlockBox : IAaBbBox
    {
        public IBlockShape Shape { get; }

        public IShape GetShape()
        {
            return Shape;
        }

        public Zone Zone { get; set; }


        public BlockBox(Zone zone, IBlockShape shape)
        {
            Zone = zone;
            Shape = shape;
        }

        private bool IsNotTouch(BlockBox another)
        {
            return Zone.NotCross(another.Zone);
        }


        public List<BlockBox> TryTouch(IEnumerable<BlockBox> aabbBoxes)
        {
            return (List<BlockBox>) aabbBoxes.Where(aabbBox => !aabbBox.IsNotTouch(this));
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

        public List<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical)
        {
            var z1234 = new List<(int, IAaBbBox)>();
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
                {
//
#if DEBUG
                    Console.Out.WriteLine($"{z.LogSide()}");
                    Console.Out.WriteLine($"{Shape.Log()}:::{vertical}");
#endif

                    var (lZones, rZones) = Shape.CutByV(vertical, z);

//                            Console.Out.WriteLine("L:::" + lZones.Count);
//                            lZones.ForEach(zzz => { Console.Out.WriteLine(SomeTools.ZoneLog(zzz)); });
//
//                            rZones.ForEach(zzz => { Console.Out.WriteLine(SomeTools.ZoneLog(zzz)); });
                    if (lZones != null)
                    {
                        z1234.Add((2, new BlockBox(lZones.Value, Shape)));
                        z1234.Add((1, new BlockBox(rZones!.Value, Shape)));
                    }
                    else
                    {
                        throw new Exception($"lz no good zone{z.LogSide()} v {vertical} {z.Right <= vertical}");
                    }
                }
            }
            else if (z.Up <= horizon)
            {
                if (z.Left >= vertical)
                    z1234.Add((4, this));
                else if (z.Right <= vertical)
                    z1234.Add((3, this));
                else
                {
                    var (llZones, rrZones) = Shape.CutByV(vertical, z);
                    if (llZones != null)
                    {
                        z1234.Add((3, new BlockBox(llZones.Value, Shape)));
                        z1234.Add((4, new BlockBox(rrZones!.Value, Shape)));
                    }
                    else
                    {
                        throw new Exception($"lz2 no good zone {Shape.Log()} cut by v {vertical} ");
                    }
                }
            }
            else
            {
                if (z.Left >= vertical)
                {
                    var (uz, dz) = Shape.CutByH(horizon, z);
//                            Console.Out.WriteLine("out::" + SomeTools.ZoneLog(uz.Value) + "AND" +
//                                                  SomeTools.ZoneLog(dz.Value));

                    if (uz != null)
                    {
                        var t1 = (1, new BlockBox(uz.Value, Shape));
                        var t2 = (4, new BlockBox(dz!.Value, Shape));
                        z1234.Add(t1);
                        z1234.Add(t2);
                    }
                }
                else if (z.Right <= vertical)

                {
                    var (uz, dz) = Shape.CutByH(horizon, z);
//                            Console.Out.WriteLine("out::" + SomeTools.ZoneLog(uz.Value) + "AND" +
//                                                  SomeTools.ZoneLog(dz.Value));

                    if (uz != null)
                    {
                        var t1 = (2, new BlockBox(uz.Value, Shape));
                        var t2 = (3, new BlockBox(dz!.Value, Shape));
                        z1234.Add(t1);
                        z1234.Add(t2);
                    }
                }
                else
                    z1234.Add((0, this));
            }

            return z1234;
        }

        public Dictionary<Quad, BlockBox> CutTo4(float horizon, float vertical)
        {
            var z1234 = new Dictionary<Quad, BlockBox>();
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
                    z1234[Quad.One] = new BlockBox(new Zone(z.Up, z.Down, vertical, z.Right), Shape);
                    z1234[Quad.Two] = new BlockBox(new Zone(z.Up, z.Down, z.Left, vertical), Shape);
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
                    z1234[Quad.Four] = new BlockBox(new Zone(z.Up, z.Down, vertical, z.Right), Shape);
                    z1234[Quad.Three] = new BlockBox(new Zone(z.Up, z.Down, z.Left, vertical), Shape);
                }
            }
            else
            {
                if (z.Left >= vertical)
                {
                    z1234[Quad.One] = new BlockBox(new Zone(z.Up, horizon, z.Left, z.Right), Shape);
                    z1234[Quad.Four] = new BlockBox(new Zone(horizon, z.Down, z.Left, z.Right), Shape);
                }
                else if (z.Right <= vertical)
                {
                    z1234[Quad.Two] = new BlockBox(new Zone(z.Up, horizon, z.Left, z.Right), Shape);
                    z1234[Quad.Three] = new BlockBox(new Zone(horizon, z.Down, z.Left, z.Right), Shape);
                }
                else
                {
                    z1234[Quad.One] = new BlockBox(new Zone(z.Up, horizon, vertical, z.Right), Shape);
                    z1234[Quad.Two] = new BlockBox(new Zone(z.Up, horizon, z.Left, vertical), Shape);
                    z1234[Quad.Three] = new BlockBox(new Zone(horizon, z.Down, z.Left, vertical), Shape);
                    z1234[Quad.Four] = new BlockBox(new Zone(horizon, z.Down, vertical, z.Right), Shape);
                }
            }


            return z1234;
        }
    }
}