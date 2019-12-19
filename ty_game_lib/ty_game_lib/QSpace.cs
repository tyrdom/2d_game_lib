#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ty_game_lib
{
    public abstract class QSpace
    {
        public virtual Quad Quad { get; set; }
        public abstract void InsertBox(AabbPackBox packBox);
        public abstract Zone Zone { get; set; }
        public abstract List<AabbPackBox> AabbPackBoxes { get; set; }
        public abstract void Remove(AabbPackBox packBox);
        public abstract IEnumerable<AabbPackBox> TouchBy(AabbPackBox packBox);
    }

    public class QSpaceBranch : QSpace
    {
        public sealed override Quad Quad { get; set; }
        public sealed override Zone Zone { get; set; }

        public sealed override List<AabbPackBox> AabbPackBoxes { get; set; }
        private QSpace RightUp { get; set; }
        private QSpace LeftUp { get; set; }

        private QSpace LeftDown { get; set; }


        private QSpace RightDown { get; set; }


        public QSpaceBranch(Zone zone, List<AabbPackBox> aabbBoxes, QSpace rightUp, QSpace leftUp,
            QSpace leftDown, QSpace rightDown
        )
        {
            Zone = zone;
            AabbPackBoxes = aabbBoxes;
            RightUp = rightUp;
            LeftUp = leftUp;
            LeftDown = leftDown;
            RightDown = rightDown;
        }

        public override void Remove(AabbPackBox packBox)
        {
            if (AabbPackBoxes.Remove(packBox)) return;

            RightUp.Remove(packBox);
            RightDown.Remove(packBox);
            LeftDown.Remove(packBox);
            LeftUp.Remove(packBox);
        }

        public override IEnumerable<AabbPackBox> TouchBy(AabbPackBox packBox)
        {
            var aabbBoxes = packBox.TryTouch(this.AabbPackBoxes);
            var (item1, item2) = Zone.GetMids();
            var cutTo4 = packBox.CutTo4(item1, item2);
            foreach (var keyValuePair in cutTo4)
            {
                switch (keyValuePair.Key)
                {
                    case Quad.One:
                        var touchBy = LeftUp.TouchBy(packBox);
                        aabbBoxes.AddRange(touchBy);
                        break;
                    case Quad.Two:
                        var boxes = RightUp.TouchBy(packBox);
                        aabbBoxes.AddRange(boxes);
                        break;
                    case Quad.Three:
                        var list = RightDown.TouchBy(packBox);
                        aabbBoxes.AddRange(list);
                        break;
                    case Quad.Four:
                        var by = LeftDown.TouchBy(packBox);
                        aabbBoxes.AddRange(by);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return aabbBoxes;
        }

        public override void InsertBox(AabbPackBox packBox)
        {
            var (item1, item2) = Zone.GetMids();
            var cutTo4 = packBox.WhichQ(item1, item2);
            switch (cutTo4)
            {
                case Quad.One:
                    RightUp.InsertBox(packBox);
                    break;
                case Quad.Two:
                    LeftUp.InsertBox(packBox);
                    break;
                case Quad.Three:
                    LeftDown.InsertBox(packBox);
                    break;
                case Quad.Four:
                    RightDown.InsertBox(packBox);
                    break;
                case null:
                    AabbPackBoxes.Add(packBox);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public QuadSpaceLeaf CovToLeaf()
        {
            var qSpaces = new[] {RightUp, LeftUp, LeftDown, RightDown};
            var a = AabbPackBoxes;
            foreach (var qSpace in qSpaces)
            {
                a.AddRange(qSpace.AabbPackBoxes);
            }

            return new QuadSpaceLeaf(Quad, Zone, a);
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

        public bool NotCross(Zone anotherZone)
        {
            return Right < anotherZone.Left || anotherZone.Right < Left ||
                   Up < anotherZone.Down || anotherZone.Up < Down;
        }

        public Zone Join(Zone another)
        {
            var nUp = MathF.Max(Up, another.Up);
            var nLeft = MathF.Min(Left, another.Left);
            var nDown = MathF.Min(Down, another.Down);
            var nRight = MathF.Max(Right, another.Right);
            return new Zone(nUp, nDown, nLeft, nRight);
        }
    }


    public class QuadSpaceLeaf : QSpace
    {
        public QuadSpaceLeaf(Quad quad, Zone zone, List<AabbPackBox> aabbPackPackBoxes)
        {
            Quad = quad;
            Zone = zone;
            AabbPackBoxes = aabbPackPackBoxes;
        }

        public sealed override Quad Quad { get; set; }
        public sealed override Zone Zone { get; set; }

        public sealed override List<AabbPackBox> AabbPackBoxes { get; set; }

        public override void Remove(AabbPackBox packBox)
        {
        }

        public override IEnumerable<AabbPackBox> TouchBy(AabbPackBox packBox)
        {
            return packBox.TryTouch(AabbPackBoxes);
        }

        public override void InsertBox(AabbPackBox packBox)
        {
            AabbPackBoxes.Add(packBox);
        }

        public QSpace TryCovToBranch()
        {
            var one = new List<AabbPackBox>();
            var two = new List<AabbPackBox>();
            var three = new List<AabbPackBox>();
            var four = new List<AabbPackBox>();
            var zone = new List<AabbPackBox>();
            var (item1, item2) = Zone.GetMids();
            Parallel.ForEach(AabbPackBoxes, mark =>
                {
                    switch (mark.WhichQ(item1, item2))
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

            if (zone == AabbPackBoxes) return this;


            var zones = Zone.CutTo4(item1, item2);


            return new QSpaceBranch(Zone, zone, new QuadSpaceLeaf(Quad.One, zones[0], one),
                new QuadSpaceLeaf(Quad.Two, zones[1], two),
                new QuadSpaceLeaf(Quad.Three, zones[2], three), new QuadSpaceLeaf(Quad.Four, zones[3], four));
        }
    }

    public enum Quad
    {
        One,
        Two,
        Three,
        Four
    }

    public class AabbPackBox
    {
        public Zone Zone;
        private readonly Shape _shape;


        public AabbPackBox(Zone zone, Shape shape)
        {
            Zone = zone;
            _shape = shape;
        }

        private bool IsNotTouch(AabbPackBox another)
        {
            return Zone.NotCross(another.Zone);
        }

        public List<AabbPackBox> TryTouch(List<AabbPackBox> aabbBoxes)
        {
            return (List<AabbPackBox>) aabbBoxes.Where(aabbBox => !aabbBox.IsNotTouch(this));
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

        public Dictionary<Quad, AabbPackBox?> CutTo4(float horizon, float vertical)
        {
            var z1234 = new Dictionary<Quad, AabbPackBox?>();
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
                    z1234[Quad.One] = new AabbPackBox(new Zone(z.Up, z.Down, vertical, z.Right), _shape);
                    z1234[Quad.Two] = new AabbPackBox(new Zone(z.Up, z.Down, z.Left, vertical), _shape);
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
                    z1234[Quad.Four] = new AabbPackBox(new Zone(z.Up, z.Down, vertical, z.Right), _shape);
                    z1234[Quad.Three] = new AabbPackBox(new Zone(z.Up, z.Down, z.Left, vertical), _shape);
                }
            }
            else
            {
                if (z.Left >= vertical)
                {
                    z1234[Quad.One] = new AabbPackBox(new Zone(z.Up, horizon, z.Left, z.Right), _shape);
                    z1234[Quad.Four] = new AabbPackBox(new Zone(horizon, z.Down, z.Left, z.Right), _shape);
                }
                else if (z.Right <= vertical)
                {
                    z1234[Quad.Two] = new AabbPackBox(new Zone(z.Up, horizon, z.Left, z.Right), _shape);
                    z1234[Quad.Three] = new AabbPackBox(new Zone(horizon, z.Down, z.Left, z.Right), _shape);
                }
                else
                {
                    z1234[Quad.One] = new AabbPackBox(new Zone(z.Up, horizon, vertical, z.Right), _shape);
                    z1234[Quad.Two] = new AabbPackBox(new Zone(z.Up, horizon, z.Left, vertical), _shape);
                    z1234[Quad.Three] = new AabbPackBox(new Zone(horizon, z.Down, z.Left, vertical), _shape);
                    z1234[Quad.Four] = new AabbPackBox(new Zone(horizon, z.Down, vertical, z.Right), _shape);
                }
            }


            return z1234;
        }
    }
}