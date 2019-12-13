using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ty_game_lib
{
    public abstract class QSpace
    {
        public virtual Quad Quad { get; set; }
        public abstract void InsertBox(AabbBox box);
        public abstract Zone Zone { get; set; }
        public abstract List<AabbBox> AabbBoxes { get; set; }
    }

    public class QSpaceBranch : QSpace
    {
        public sealed override Quad Quad { get; set; }
        public sealed override Zone Zone { get; set; }

        public sealed override List<AabbBox> AabbBoxes { get; set; }
        private QSpace RightUp { get; set; }
        private QSpace LeftUp { get; set; }

        private QSpace LeftDown { get; set; }


        private QSpace RightDown { get; set; }


        public QSpaceBranch(Zone zone, List<AabbBox> aabbBoxes, QSpace rightUp, QSpace leftUp,
            QSpace leftDown, QSpace rightDown
        )
        {
            Zone = zone;
            AabbBoxes = aabbBoxes;
            RightUp = rightUp;
            LeftUp = leftUp;
            LeftDown = leftDown;
            RightDown = rightDown;
        }

        public override void InsertBox(AabbBox box)
        {
            var (item1, item2) = Zone.GetMids();
            var cutTo4 = box.WichQ(item1, item2);
            switch (cutTo4)
            {
                case Quad.One:
                    RightUp.InsertBox(box);
                    break;
                case Quad.Two:
                    LeftUp.InsertBox(box);
                    break;
                case Quad.Three:
                    LeftDown.InsertBox(box);
                    break;
                case Quad.Four:
                    RightDown.InsertBox(box);
                    break;
                case null:
                    AabbBoxes.Add(box);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public QSpaceLeaf CovToLeaf()
        {
            var qSpaces = new[] {RightUp, LeftUp, LeftDown, RightDown};
            var a = AabbBoxes;
            foreach (var qSpace in qSpaces)
            {
                a.AddRange(qSpace.AabbBoxes);
            }

            return new QSpaceLeaf(Quad, Zone, a);
        }
    }


    public struct Zone
    {
        public readonly float Up;
        public readonly float Down;
        public readonly float Left;
        public readonly float Right;

        public Zone(float up, float down, float left, float right)
        {
            Up = up;
            Down = down;
            Left = left;
            Right = right;
        }

        public (float, float) GetMids()
        {
            var horizon = (Up + Down) / 2;
            var vertical = (Right + Left) / 2;
            return (horizon, vertical);
        }

        public Zone[] CutTo4(float horizon, float vertical)
        {
            var z1 = new Zone(Up, horizon, vertical, Right);
            var z2 = new Zone(Up, horizon, Left, vertical);
            var z3 = new Zone(horizon, Down, Left, vertical);
            var z4 = new Zone(horizon, Down, vertical, Right);
            return new Zone[4] {z1, z2, z3, z4};
        }

        bool IsIn(Zone anotherZone)
        {
            return anotherZone.Left <= Left && anotherZone.Right >= Right && anotherZone.Up >= Up &&
                   anotherZone.Down <= Down;
        }

        bool Cross(Zone anotherZone)
        {
            return !(Right < anotherZone.Left || anotherZone.Right < Left ||
                     Up < anotherZone.Down || anotherZone.Up < Down);
        }
    }


    public class QSpaceLeaf : QSpace
    {
        public QSpaceLeaf(Quad quad, Zone zone, List<AabbBox> aabbBoxes)
        {
            Quad = quad;
            Zone = zone;
            AabbBoxes = aabbBoxes;
        }

        public sealed override Quad Quad { get; set; }
        public sealed override Zone Zone { get; set; }

        public sealed override List<AabbBox> AabbBoxes { get; set; }


        public override void InsertBox(AabbBox box)
        {
            AabbBoxes.Add(box);
        }

        public QSpace TryCovToBranch()
        {
            var one = new List<AabbBox>();
            var two = new List<AabbBox>();
            var three = new List<AabbBox>();
            var four = new List<AabbBox>();
            var zone = new List<AabbBox>();
            var (item1, item2) = Zone.GetMids();
            Parallel.ForEach(AabbBoxes, mark =>
                {
                    switch (mark.WichQ(item1, item2))
                    {
                        case Quad.One:
                            one.Add(mark);
                            break;
                        case Quad.Two:
                            two.Add(mark);
                            break;
                        case Quad.Three:
                            three.Add(mark);
                            break;
                        case Quad.Four:
                            four.Add(mark);
                            break;
                        case null:
                            zone.Add(mark);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            );

            if (zone == AabbBoxes) return this;


            var zones = Zone.CutTo4(item1, item2);


            return new QSpaceBranch(Zone, zone, new QSpaceLeaf(Quad.One, zones[0], one),
                new QSpaceLeaf(Quad.Two, zones[1], two),
                new QSpaceLeaf(Quad.Three, zones[2], three), new QSpaceLeaf(Quad.Four, zones[3], four));
        }
    }

    public enum Quad
    {
        One,
        Two,
        Three,
        Four
    }

    public class AabbBox
    {
        Zone _zone;
        private int Shape;


        public AabbBox(Zone zone, int shape)
        {
            _zone = zone;
            Shape = shape;
        }

        public Quad? WichQ(float horizon, float vertical)
        {
            if (_zone.Down >= horizon)
            {
                if (_zone.Left >= vertical) return Quad.One;
                if (_zone.Right <= vertical) return Quad.Two;
            }

            if (_zone.Up <= horizon)
            {
                if (_zone.Left >= vertical) return Quad.Four;
                if (_zone.Right <= vertical) return Quad.Three;
            }

            return null;
        }

        public AabbBox?[] CutTo4(float horizon, float vertical)
        {
            var z1234 = new AabbBox?[4];
            var z = _zone;


            if (z.Down >= horizon)
            {
                if (z.Left >= vertical)
                {
                    z1234[0] = this;
                }
                else if (z.Right <= vertical)
                {
                    z1234[1] = this;
                }
                else
                {
                    z1234[0] = new AabbBox(new Zone(z.Up, z.Down, vertical, z.Right), Shape);
                    z1234[1] = new AabbBox(new Zone(z.Up, z.Down, z.Left, vertical), Shape);
                }
            }
            else if (z.Up <= horizon)
            {
                if (z.Left >= vertical)
                {
                    z1234[3] = this;
                }
                else if (z.Right <= vertical)
                {
                    z1234[2] = this;
                }
                else
                {
                    z1234[3] = new AabbBox(new Zone(z.Up, z.Down, vertical, z.Right), Shape);
                    z1234[2] = new AabbBox(new Zone(z.Up, z.Down, z.Left, vertical), Shape);
                }
            }
            else
            {
                if (z.Left >= vertical)
                {
                    z1234[0] = new AabbBox(new Zone(z.Up, horizon, z.Left, z.Right), Shape);
                    z1234[3] = new AabbBox(new Zone(horizon, z.Down, z.Left, z.Right), Shape);
                }
                else if (z.Right <= vertical)
                {
                    z1234[1] = new AabbBox(new Zone(z.Up, horizon, z.Left, z.Right), Shape);
                    z1234[2] = new AabbBox(new Zone(horizon, z.Down, z.Left, z.Right), Shape);
                }
                else
                {
                    z1234[0] = new AabbBox(new Zone(z.Up, horizon, vertical, z.Right), Shape);
                    z1234[1] = new AabbBox(new Zone(z.Up, horizon, z.Left, vertical), Shape);
                    z1234[2] = new AabbBox(new Zone(horizon, z.Down, z.Left, vertical), Shape);
                    z1234[3] = new AabbBox(new Zone(horizon, z.Down, vertical, z.Right), Shape);
                }
            }


            return z1234;
        }
    }
}