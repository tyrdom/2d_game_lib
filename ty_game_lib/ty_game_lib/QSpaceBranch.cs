using System;
using System.Collections.Generic;

namespace ty_game_lib
{
    public class QSpaceBranch : QSpace
    {
        public QSpaceBranch(Quad? quad, Zone zone, List<AabbBoxShape> aabbPackBoxes, QSpace rightUp, QSpace leftUp,
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

        public sealed override Quad? TheQuad { get; set; }

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
            var (item1, item2) = Zone.GetMid();
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
            var (item1, item2) = Zone.GetMid();
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
}