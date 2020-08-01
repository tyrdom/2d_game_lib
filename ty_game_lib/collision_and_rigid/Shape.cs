using System.Collections.Generic;
using System.Dynamic;

namespace collision_and_rigid
{
    public interface IShape
    {
    }

    public interface IRawBulletShape
    {
        Zone GenBulletZone(float r);

        IBulletShape GenBulletShape(float r);
    }


    public interface IBulletShape
    {
        public bool PtInShape(TwoDPoint point);
    }

    public interface ITwoDTwoP
    {
        string Log();
    }

    public interface IIdPointShape : IShape
    {
        int GetId();
        TwoDPoint Move(ITwoDTwoP vector);
        TwoDPoint GetAnchor();
    }

    public interface IBlockShape
    {
        int TouchByRightShootPointInAAbbBox(TwoDPoint p);
        bool IsEmpty();


        List<(TwoDPoint, CondAfterCross, CondAfterCross)> CrossAnotherBlockShapeReturnCrossPtAndThisCondAnotherCond(
            IBlockShape blockShape);

        TwoDPoint GetStartPt();
        TwoDPoint GetEndPt();

        (List<IBlockShape>, CondAfterCross, List<IBlockShape>) CutByPointReturnGoodBlockCondAndTemp(
            CondAfterCross nowCond,
            List<(TwoDPoint, CondAfterCross)>? ptsAndCond, List<IBlockShape> temp, CondAfterCross endCond);

        bool CheckAfter(IBlockShape another);

        bool CheckBefore(IBlockShape another);
    }


    public enum CondAfterCross
    {
        OutToIn,
        InToOut,
        MaybeOutToIn
    }
}