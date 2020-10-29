using System;
using System.Collections.Generic;
using System.Linq;

namespace collision_and_rigid
{
    public struct Zone
    {
        public float Up;
        public float Down;
        public float Left;
        public float Right;

        public Zone(float up, float down, float left, float right)
        {
            Up = up;
            Down = down;
            Left = left;
            Right = right;
        }

        public Zone GetBulletRdBox()
        {
            var floats = new[] {MathTools.Abs(Up), MathTools.Abs(Down), MathTools.Abs(Left), MathTools.Abs(Right)};
            var max = floats.Max();
            var zone = new Zone(max, -max, -max, max);
            return zone;
        }

        public TwoDPoint RU()
        {
            return new TwoDPoint(Right, Up);
        }

        public TwoDPoint RD()
        {
            return new TwoDPoint(Right, Down);
        }

        public TwoDPoint LU()
        {
            return new TwoDPoint(Left, Up);
        }

        public TwoDPoint LD()
        {
            return new TwoDPoint(Left, Down);
        }

        public (float horizon, float vertical) GetMid()
        {
            var horizon = (Up + Down) / 2;
            var vertical = (Right + Left) / 2;
            return (horizon, vertical);
        }

        public Zone[] CutTo4()
        {
            var (horizon, vertical) = GetMid();
            var z1 = new Zone(Up, horizon, vertical, Right);
            var z2 = new Zone(Up, horizon, Left, vertical);
            var z3 = new Zone(horizon, Down, Left, vertical);
            var z4 = new Zone(horizon, Down, vertical, Right);
            return new Zone[4] {z1, z2, z3, z4};
        }

        public Zone[] CutTo4(float horizon, float vertical)
        {
            var z1 = new Zone(Up, horizon, vertical, Right);
            var z2 = new Zone(Up, horizon, Left, vertical);
            var z3 = new Zone(horizon, Down, Left, vertical);
            var z4 = new Zone(horizon, Down, vertical, Right);
            return new Zone[4] {z1, z2, z3, z4};
        }

        public List<(int, Zone)> SplitByQuads(float horizon, float vertical)
        {
            var valueTuples = new List<(int, Zone)>();
            if (Left >= vertical)
            {
                if (Down >= horizon)
                {
                    valueTuples.Add((1, this));
                }

                else if (Up <= horizon)
                {
                    valueTuples.Add((4, this));
                }
                else
                {
                    var (up, down) = CutByH(horizon);
                    valueTuples.Add((1, up));
                    valueTuples.Add((4, down));
                }
            }

            if (Right <= vertical)
            {
                if (Down >= horizon)
                {
                    valueTuples.Add((2, this));
                }

                else if (Up <= horizon)
                {
                    valueTuples.Add((3, this));
                }
                else
                {
                    var (up, down) = CutByH(horizon);
                    valueTuples.Add((2, up));
                    valueTuples.Add((3, down));
                }
            }
            else
            {
                if (Down >= horizon)
                {
                    var (left, right) = CutByV(vertical);

                    valueTuples.Add((2, left));
                    valueTuples.Add((1, right));
                }

                else if (Up <= horizon)
                {
                    var (left, right) = CutByV(vertical);

                    valueTuples.Add((3, left));
                    valueTuples.Add((4, right));
                }
                else
                {
                    valueTuples.Add((0, this));
                }
            }

            return valueTuples;
        }

        public List<(Quad quad, Zone zone)> SplitZones(Zone zone)
        {
            var list = new List<(Quad quad, Zone zone)>();
            var (horizon, vertical) = GetMid();

            if (zone.Down >= horizon)
            {
                if (zone.Right <= vertical)
                {
                    list.Add((Quad.Three, zone));
                }
                else if (zone.Left >= vertical)
                {
                    list.Add((Quad.Four, zone));
                }
                else
                {
                    var (left, right) = zone.CutByV(vertical);
                    list.Add((Quad.Three, left));
                    list.Add((Quad.Four, right));
                }
            }
            else if (zone.Up <= horizon)
            {
                if (zone.Right <= vertical)
                {
                    list.Add((Quad.Two, zone));
                }
                else if (zone.Left >= vertical)
                {
                    list.Add((Quad.One, zone));
                }
                else
                {
                    var (left, right) = zone.CutByV(vertical);
                    list.Add((Quad.Two, left));
                    list.Add((Quad.One, right));
                }
            }
            else
            {
                if (zone.Right <= vertical)
                {
                    var (up, down) = zone.CutByH(horizon);
                    list.Add((Quad.Two, up));
                    list.Add((Quad.Three, down));
                }
                else if (zone.Left >= vertical)
                {
                    var (up, down) = zone.CutByH(horizon);
                    list.Add((Quad.One, up));
                    list.Add((Quad.Four, down));
                }
                else
                {
                    var cutTo4 = zone.CutTo4(horizon, vertical);
                    list.Add((Quad.One, cutTo4[0]));
                    list.Add((Quad.Two, cutTo4[1]));
                    list.Add((Quad.Three, cutTo4[2]));
                    list.Add((Quad.Four, cutTo4[3]));
                }
            }

            return list;
        }

        public (Zone up, Zone down) CutByH(float horizon)
        {
            var z2 = new Zone(Up, horizon, Left, Right);
            var z3 = new Zone(horizon, Down, Left, Right);
            return (z2, z3);
        }

        public (Zone left, Zone right) CutByV(float vertical)
        {
            var z3 = new Zone(Up, Down, Left, vertical);
            var z4 = new Zone(Up, Down, vertical, Right);
            return (z3, z4);
        }

        public bool IsIn(Zone anotherZone)
        {
            return anotherZone.Left <= Left && anotherZone.Right >= Right && anotherZone.Up >= Up &&
                   anotherZone.Down <= Down;
        }

        public bool RealNotCross(Zone anotherZone)
        {
            return Right < anotherZone.Left || anotherZone.Right < Left ||
                   Up < anotherZone.Down || anotherZone.Up < Down;
        }

        public bool NotCross(Zone anotherZone)
        {
            if (Right <= Left)
            {
                return Right < anotherZone.Left || anotherZone.Right < Left ||
                       Up <= anotherZone.Down || anotherZone.Up <= Down;
            }

            if (Up <= Down)
            {
                return Right <= anotherZone.Left || anotherZone.Right <= Left ||
                       Up < anotherZone.Down || anotherZone.Up < Down;
            }

            return Right <= anotherZone.Left || anotherZone.Right <= Left ||
                   Up <= anotherZone.Down || anotherZone.Up <= Down;
        }

        public string LogSide()
        {
            return $"[{Up}|{Down}|{Left}|{Right}]";
        }

        public Zone Inter(Zone another)
        {
            var nUp = Math.Min(Up, another.Up);
            var nLeft = Math.Max(Left, another.Left);
            var nDown = Math.Max(Down, another.Down);
            var nRight = Math.Min(Right, another.Right);
            return new Zone(nUp, nDown, nLeft, nRight);
        }

        public static Zone Zero()
        {
            return new Zone(0f, 0f, 0f, 0f);
        }


        public static Zone Join(Zone? a, Zone b)
        {
            return a?.Join(b) ?? b;
        }

        public Zone Join(Zone another)
        {
            var nUp = MathTools.Max(Up, another.Up);
            var nLeft = MathTools.Min(Left, another.Left);
            var nDown = MathTools.Min(Down, another.Down);
            var nRight = MathTools.Max(Right, another.Right);


            return new Zone(nUp, nDown, nLeft, nRight);
        }

        public bool IncludePt(TwoDPoint pt)
        {
            var x = pt.X;
            var y = pt.Y;
            return Up >= y && y >= Down && x >= Left && Right >= x;
        }

        public Zone MoreWide(float w)
        {
            return new Zone(Up, Down, Left - w, Right + w);
        }

        public Zone MoreHigh(float w)
        {
            return new Zone(Up + w, Down - w, Left, Right);
        }

        public Poly ToPoly()
        {
            var twoDPoints = new[] {LD(), LU(), RU(), RD()};
            var poly = new Poly(twoDPoints);
            return poly;
        }

        public Zone ClockTurnAboutZero(TwoDVector aim)
        {
            var genZone = ToPoly().ClockTurnAboutZero(aim).GenZone();
            return genZone;
        }

        public Zone MoveToAnchor(TwoDPoint anchor)
        {
            return new Zone(Up + anchor.Y, Down + anchor.Y, Left + anchor.X, Right + anchor.X);
        }
    }
}