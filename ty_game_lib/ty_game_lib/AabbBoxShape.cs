using System.Collections.Generic;
using System.Linq;

namespace ty_game_lib
{
    public class AabbBoxShape
    {
        public Zone Zone;
        private readonly Shape _shape;


        public AabbBoxShape(Zone zone, Shape shape)
        {
            Zone = zone;
            _shape = shape;
        }

        private bool IsNotTouch(AabbBoxShape another)
        {
            return Zone.NotCross(another.Zone);
        }

        public List<AabbBoxShape> TryTouch(List<AabbBoxShape> aabbBoxes)
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

        public Dictionary<int, AabbBoxShape> SplitByQuads(float horizon, float vertical)
        {
            var z1234 = new Dictionary<int, AabbBoxShape>();
            var z = Zone;


            if (z.Down >= horizon)
            {
                if (z.Left >= vertical)
                {
                    z1234[1] = this;
                }
                else if (z.Right <= vertical)
                {
                    z1234[2] = this;
                }
                else
                {
                    z1234[1] = new AabbBoxShape(new Zone(z.Up, z.Down, vertical, z.Right), _shape);
                    z1234[2] = new AabbBoxShape(new Zone(z.Up, z.Down, z.Left, vertical), _shape);
                }
            }
            else if (z.Up <= horizon)
            {
                if (z.Left >= vertical)
                {
                    z1234[4] = this;
                }
                else if (z.Right <= vertical)
                {
                    z1234[3] = this;
                }
                else
                {
                    z1234[4] = new AabbBoxShape(new Zone(z.Up, z.Down, vertical, z.Right), _shape);
                    z1234[3] = new AabbBoxShape(new Zone(z.Up, z.Down, z.Left, vertical), _shape);
                }
            }
            else
            {
                if (z.Left >= vertical)
                {
                    z1234[1] = new AabbBoxShape(new Zone(z.Up, horizon, z.Left, z.Right), _shape);
                    z1234[4] = new AabbBoxShape(new Zone(horizon, z.Down, z.Left, z.Right), _shape);
                }
                else if (z.Right <= vertical)
                {
                    z1234[2] = new AabbBoxShape(new Zone(z.Up, horizon, z.Left, z.Right), _shape);
                    z1234[3] = new AabbBoxShape(new Zone(horizon, z.Down, z.Left, z.Right), _shape);
                }
                else
                {
                    z1234[0] = this;
                }
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
                    z1234[Quad.One] = new AabbBoxShape(new Zone(z.Up, z.Down, vertical, z.Right), _shape);
                    z1234[Quad.Two] = new AabbBoxShape(new Zone(z.Up, z.Down, z.Left, vertical), _shape);
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
                    z1234[Quad.Four] = new AabbBoxShape(new Zone(z.Up, z.Down, vertical, z.Right), _shape);
                    z1234[Quad.Three] = new AabbBoxShape(new Zone(z.Up, z.Down, z.Left, vertical), _shape);
                }
            }
            else
            {
                if (z.Left >= vertical)
                {
                    z1234[Quad.One] = new AabbBoxShape(new Zone(z.Up, horizon, z.Left, z.Right), _shape);
                    z1234[Quad.Four] = new AabbBoxShape(new Zone(horizon, z.Down, z.Left, z.Right), _shape);
                }
                else if (z.Right <= vertical)
                {
                    z1234[Quad.Two] = new AabbBoxShape(new Zone(z.Up, horizon, z.Left, z.Right), _shape);
                    z1234[Quad.Three] = new AabbBoxShape(new Zone(horizon, z.Down, z.Left, z.Right), _shape);
                }
                else
                {
                    z1234[Quad.One] = new AabbBoxShape(new Zone(z.Up, horizon, vertical, z.Right), _shape);
                    z1234[Quad.Two] = new AabbBoxShape(new Zone(z.Up, horizon, z.Left, vertical), _shape);
                    z1234[Quad.Three] = new AabbBoxShape(new Zone(horizon, z.Down, z.Left, vertical), _shape);
                    z1234[Quad.Four] = new AabbBoxShape(new Zone(horizon, z.Down, vertical, z.Right), _shape);
                }
            }


            return z1234;
        }
    }
}