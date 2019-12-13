using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ty_game_lib
{
    public abstract class QSpace
    {
        public abstract void InsertBox(AabbBox box);
        public abstract Zone Zone { get; set; }
        public abstract List<AabbBox> AabbBoxes { get; set; }
    }

    public class QSpaceBranch : QSpace
    {
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
            var cutTo4 = box.CutTo4(Zone, item1, item2);
            AabbBoxes.Add(cutTo4[0]);
            RightUp.InsertBox(cutTo4[1]);
            LeftUp.InsertBox(cutTo4[2]);
            RightDown.InsertBox(cutTo4[3]);
            LeftDown.InsertBox(cutTo4[4]);
        }

        public QSpaceLeaf CovToLeaf()
        {
            var qSpaces = new[] {RightUp, LeftUp, LeftDown, RightDown};
            var a = AabbBoxes;
            foreach (var qSpace in qSpaces)
            {
                a.AddRange(qSpace.AabbBoxes);
            }

            return new QSpaceLeaf(Zone, a);
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
        public sealed override Zone Zone { get; set; }

        public sealed override List<AabbBox> AabbBoxes { get; set; }

        public QSpaceLeaf(Zone zone, List<AabbBox> aabbBoxes)
        {
            this.Zone = zone;
            this.AabbBoxes = aabbBoxes;
        }

        public override void InsertBox(AabbBox box)
        {
            AabbBoxes.Add(box);
        }

        QSpaceBranch CovToBranch()
        {
            var z = new List<AabbBox>[5];

            var (item1, item2) = Zone.GetMids();
            Parallel.ForEach(AabbBoxes, new Action<AabbBox>(mark =>
                    {
                        var cutTo4 = mark.CutTo4(Zone, item1, item2);
                        foreach (var i
                            in Enumerable.Range(0, cutTo4.Length))
                        {
                            var st = cutTo4[i];
                            if (st != null)
                            {
                                z[i].Add(st);
                            }
                        }
                    }
                )
            );

            {
            }
            var zones = Zone.CutTo4(item1, item2);

            return new QSpaceBranch(Zone, z[0], new QSpaceLeaf(zones[0], z[1]), new QSpaceLeaf(zones[1], z[2]),
                new QSpaceLeaf(zones[2], z[3]), new QSpaceLeaf(zones[3], z[4]));
        }
    }

    public class AabbBox
    {
        Zone _zone;

        public AabbBox(Zone zone)
        {
            _zone = zone;
        }

        public AabbBox?[] CutTo4(Zone zone, float horizon, float vertical)
        {
            var z1234 = new AabbBox?[5];
            var z = _zone;


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
                    z1234[1] = new AabbBox(new Zone(z.Up, z.Down, vertical, z.Right));
                    z1234[2] = new AabbBox(new Zone(z.Up, z.Down, z.Left, vertical));
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
                    z1234[4] = new AabbBox(new Zone(z.Up, z.Down, vertical, z.Right));
                    z1234[3] = new AabbBox(new Zone(z.Up, z.Down, z.Left, vertical));
                }
            }
            else
            {
                if (z.Left >= vertical)
                {
                    z1234[1] = new AabbBox(new Zone(z.Up, horizon, z.Left, z.Right));
                    z1234[4] = new AabbBox(new Zone(horizon, z.Down, z.Left, z.Right));
                }
                else if (z.Right <= vertical)
                {
                    z1234[2] = new AabbBox(new Zone(z.Up, horizon, z.Left, z.Right));
                    z1234[3] = new AabbBox(new Zone(horizon, z.Down, z.Left, z.Right));
                }
                else
                {
                    z1234[0] = this;
                }
            }


            return z1234;
        }
    }
}