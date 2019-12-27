using System;
using System.Collections.Generic;

namespace ty_game_lib
{
    public class QSpaceBranch : QSpace
    {
        public QSpaceBranch(Quad? quad, Zone zone, List<AabbBoxShape> aabbPackBoxes, QSpace quadTwo, QSpace quadOne,
            QSpace quadFour, QSpace quadThree)
        {
            TheQuad = quad;
            Zone = zone;
            AabbPackBoxes = aabbPackBoxes;
            QuadTwo = quadTwo;
            QuadOne = quadOne;
            QuadFour = quadFour;
            QuadThree = quadThree;
        }

        public sealed override Quad? TheQuad { get; set; }

        public sealed override Zone Zone { get; set; }

        public sealed override List<AabbBoxShape> AabbPackBoxes { get; set; }
        public QSpace QuadTwo { get; set; }
        public QSpace QuadOne { get; set; }

        public QSpace QuadFour { get; set; }


        public QSpace QuadThree { get; set; }


        public override void Remove(AabbBoxShape boxShape)
        {
            if (AabbPackBoxes.Remove(boxShape)) return;

            QuadTwo.Remove(boxShape);
            QuadThree.Remove(boxShape);
            QuadFour.Remove(boxShape);
            QuadOne.Remove(boxShape);
        }

        public override IEnumerable<AabbBoxShape> TouchBy(AabbBoxShape boxShape)
        {
            var aabbBoxes = boxShape.TryTouch(this.AabbPackBoxes);
            var (item1, item2) = Zone.GetMid();
            var cutTo4 = boxShape.CutTo4(item1, item2);
            foreach (var keyValuePair in cutTo4)
            {
                switch (keyValuePair.Key)
                {
                    case Quad.One:
                        var touchBy = QuadOne.TouchBy(boxShape);
                        aabbBoxes.AddRange(touchBy);
                        break;
                    case Quad.Two:
                        var boxes = QuadTwo.TouchBy(boxShape);
                        aabbBoxes.AddRange(boxes);
                        break;
                    case Quad.Three:
                        var list = QuadThree.TouchBy(boxShape);
                        aabbBoxes.AddRange(list);
                        break;
                    case Quad.Four:
                        var by = QuadFour.TouchBy(boxShape);
                        aabbBoxes.AddRange(by);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return aabbBoxes;
        }

        public override QSpace TryCovToLimitQSpace(int limit)
        {
            QSpace tryCovToLimitQSpace1 = QuadOne.TryCovToLimitQSpace(limit);
            QSpace tryCovToLimitQSpace2 = QuadTwo.TryCovToLimitQSpace(limit);
            QSpace tryCovToLimitQSpace3 = QuadThree.TryCovToLimitQSpace(limit);
            QSpace tryCovToLimitQSpace4 = QuadFour.TryCovToLimitQSpace(limit);
            return new QSpaceBranch(TheQuad, Zone, AabbPackBoxes, tryCovToLimitQSpace2, tryCovToLimitQSpace1,
                tryCovToLimitQSpace4, tryCovToLimitQSpace3);
        }

        public override (int, AabbBoxShape?) touchWithARightShootPoint(TwoDPoint p)
        {
            var qSpaces = new QSpace[] {QuadOne, QuadTwo, QuadThree, QuadFour};
            var (i, aabb) = p.GenARightShootCrossAlotAabbBoxShape(Zone, AabbPackBoxes);
            foreach (var qSpace in qSpaces)
            {
                var (item1, aabbBoxShape) = qSpace.touchWithARightShootPoint(p);
                if (aabbBoxShape != null)
                {
                    aabb = aabbBoxShape;
                }

                if (item1 < 0)
                {
                    i = item1;
                    return (i, aabb);
                }

                {
                    i += item1;
                }
            }

            return (i, aabb);
        }

        public override string outZones()
        {
            string s = "";
            foreach (var aabbBoxShape in AabbPackBoxes)
            {
                var zone = aabbBoxShape.Zone;
                s += SomeTools.ZoneLog(zone) + "\n";
            }

            var qSpaces = new QSpace[] {QuadOne, QuadTwo, QuadThree, QuadFour};
            foreach (var qSpace in qSpaces)
            {
                var zones = qSpace.outZones();
                s += zones + "\n";
            }

            return s;
        }

        public override void InsertBox(AabbBoxShape boxShape)
        {
            var (item1, item2) = Zone.GetMid();
            var cutTo4 = boxShape.WhichQ(item1, item2);
            switch (cutTo4)
            {
                case Quad.One:
                    QuadTwo.InsertBox(boxShape);
                    break;
                case Quad.Two:
                    QuadOne.InsertBox(boxShape);
                    break;
                case Quad.Three:
                    QuadFour.InsertBox(boxShape);
                    break;
                case Quad.Four:
                    QuadThree.InsertBox(boxShape);
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
            var qSpaces = new[] {QuadTwo, QuadOne, QuadFour, QuadThree};
            var a = AabbPackBoxes;
            foreach (var qSpace in qSpaces)
            {
                a.AddRange(qSpace.AabbPackBoxes);
            }

            return new QSpaceLeaf(TheQuad, Zone, a);
        }
    }
}