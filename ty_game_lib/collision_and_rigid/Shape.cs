using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace collision_and_rigid
{
    public interface IShape
    {
        AabbBoxShape CovToAabbPackBox();

        int TouchByRightShootPointInAAbbBox(TwoDPoint p);


        bool IsTouchAnother(IShape another);
    }

    public interface IBlockShape
    {
        (bool, IBlockShape[]) BlockShapeUnionInSamePloy(IBlockShape another);
        bool IsEmpty();


        List<(TwoDPoint, CondAfterCross, CondAfterCross)> CrossAnotherBlockShapeReturnCrossPtAndThisCondAnotherCond(
            IBlockShape blockShape);

        TwoDPoint GetStartPt();
        TwoDPoint GetEndPt();

        (List<IBlockShape>, CondAfterCross, List<IBlockShape>) CutByPointReturnGoodBlockCondAndTemp(CondAfterCross nowCond,
            List<(TwoDPoint, CondAfterCross)>? ptsAndCond, List<IBlockShape> temp);
    }

    public enum CondAfterCross
    {
        OutToIn,
        InToOut,
        MaybeOutToIn
    }
}