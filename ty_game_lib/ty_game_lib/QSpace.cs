#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ty_game_lib
{
    public abstract class QSpace
    {
        public virtual Quad TheQuad { get; set; }
        public abstract void InsertBox(AabbBoxShape boxShape);
        public abstract Zone Zone { get; set; }
        public abstract List<AabbBoxShape> AabbPackBoxes { get; set; }
        public abstract void Remove(AabbBoxShape boxShape);
        public abstract IEnumerable<AabbBoxShape> TouchBy(AabbBoxShape boxShape);
    }

    public class QSpaceBranch : QSpace
    {
        public QSpaceBranch(Quad quad, Zone zone, List<AabbBoxShape> aabbPackBoxes, QSpace rightUp, QSpace leftUp,
            QSpace leftDown, QSpace rightDown)
        {
            TheQuad = quad;
            Zone = zone;
            AabbPackBoxes = aabbPackBoxes;
            RightUp = rightUp;
            LeftUp = leftUp;
            LeftDown = leftDown;
            RightDown = rightDown;
        }

        public sealed override Quad TheQuad { get; set; }
        public sealed override Zone Zone { get; set; }

        public sealed override List<AabbBoxShape> AabbPackBoxes { get; set; }
        private QSpace RightUp { get; set; }
        private QSpace LeftUp { get; set; }

        private QSpace LeftDown { get; set; }


        private QSpace RightDown { get; set; }


        public override void Remove(AabbBoxShape boxShape)
        {
            if (AabbPackBoxes.Remove(boxShape)) return;

            RightUp.Remove(boxShape);
            RightDown.Remove(boxShape);
            LeftDown.Remove(boxShape);
            LeftUp.Remove(boxShape);
        }

        public override IEnumerable<AabbBoxShape> TouchBy(AabbBoxShape boxShape)
        {
            var aabbBoxes = boxShape.TryTouch(this.AabbPackBoxes);
            var (item1, item2) = Zone.GetMids();
            var cutTo4 = boxShape.CutTo4(item1, item2);
            foreach (var keyValuePair in cutTo4)
            {
                switch (keyValuePair.Key)
                {
                    case Quad.One:
                        var touchBy = LeftUp.TouchBy(boxShape);
                        aabbBoxes.AddRange(touchBy);
                        break;
                    case Quad.Two:
                        var boxes = RightUp.TouchBy(boxShape);
                        aabbBoxes.AddRange(boxes);
                        break;
                    case Quad.Three:
                        var list = RightDown.TouchBy(boxShape);
                        aabbBoxes.AddRange(list);
                        break;
                    case Quad.Four:
                        var by = LeftDown.TouchBy(boxShape);
                        aabbBoxes.AddRange(by);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return aabbBoxes;
        }

        public override void InsertBox(AabbBoxShape boxShape)
        {
            var (item1, item2) = Zone.GetMids();
            var cutTo4 = boxShape.WhichQ(item1, item2);
            switch (cutTo4)
            {
                case Quad.One:
                    RightUp.InsertBox(boxShape);
                    break;
                case Quad.Two:
                    LeftUp.InsertBox(boxShape);
                    break;
                case Quad.Three:
                    LeftDown.InsertBox(boxShape);
                    break;
                case Quad.Four:
                    RightDown.InsertBox(boxShape);
                    break;
                case null:
                    AabbPackBoxes.Add(boxShape);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public QSpaceLeaf CovToLeaf()
        {
            var qSpaces = new[] {RightUp, LeftUp, LeftDown, RightDown};
            var a = AabbPackBoxes;
            foreach (var qSpace in qSpaces)
            {
                a.AddRange(qSpace.AabbPackBoxes);
            }

            return new QSpaceLeaf(TheQuad, Zone, a);
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


    public class QSpaceLeaf : QSpace
    {
        public QSpaceLeaf(Quad quad, Zone zone, List<AabbBoxShape> aabbPackPackBoxes)
        {
            TheQuad = quad;
            Zone = zone;
            AabbPackBoxes = aabbPackPackBoxes;
        }

        public sealed override Quad TheQuad { get; set; }
        public sealed override Zone Zone { get; set; }

        public sealed override List<AabbBoxShape> AabbPackBoxes { get; set; }

        public override void Remove(AabbBoxShape boxShape)
        {
        }

        public override IEnumerable<AabbBoxShape> TouchBy(AabbBoxShape boxShape)
        {
            return boxShape.TryTouch(AabbPackBoxes);
        }

        public override void InsertBox(AabbBoxShape boxShape)
        {
            AabbPackBoxes.Add(boxShape);
        }

        public QSpace TryCovToBranch()
        {
            var one = new List<AabbBoxShape>();
            var two = new List<AabbBoxShape>();
            var three = new List<AabbBoxShape>();
            var four = new List<AabbBoxShape>();
            var zone = new List<AabbBoxShape>();
            var (item1, item2) = Zone.GetMids();
            Parallel.ForEach(AabbPackBoxes, aabbBoxShape =>
                {
                    switch (aabbBoxShape.WhichQ(item1, item2))
                    {
                        case Quad.One:
                            one.Add(aabbBoxShape);
                            break;
                        case Quad.Two:
                            two.Add(aabbBoxShape);
                            break;
                        case Quad.Three:
                            three.Add(aabbBoxShape);
                            break;
                        case Quad.Four:
                            four.Add(aabbBoxShape);
                            break;
                        case null:
                            zone.Add(aabbBoxShape);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            );

            if (zone.Count == AabbPackBoxes.Count) return this;


            var zones = Zone.CutTo4(item1, item2);


            return new QSpaceBranch(TheQuad, Zone, zone, new QSpaceLeaf(Quad.One, zones[0], one),
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

        public Dictionary<Quad, AabbBoxShape?> CutTo4(float horizon, float vertical)
        {
            var z1234 = new Dictionary<Quad, AabbBoxShape?>();
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