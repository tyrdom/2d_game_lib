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
        public bool PtRealInShape(TwoDPoint point);
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

    public interface IBlockShape : IShape
    {
        public (Zone? leftZone, Zone? rightZone) CutByV(float v, Zone z);
        public (Zone?, Zone?) CutByH(float h, Zone z);
        TwoDVectorLine AsTwoDVectorLine();

        List<BlockBox> GenAabbBoxShape();
        string Log();
        int TouchByRightShootPointInAAbbBoxInQSpace(TwoDPoint p);
        bool IsEmpty();


        List<(TwoDPoint crossPt, CondAfterCross shape1AfterCond, CondAfterCross shape2AfterCond)>
            CrossAnotherBlockShapeReturnCrossPtAndThisCondAnotherCond(
                IBlockShape blockShape);

        TwoDPoint GetStartPt();
        TwoDPoint GetEndPt();

        (List<IBlockShape>, CondAfterCross, List<IBlockShape>) CutByPointReturnGoodBlockCondAndTemp(
            CondAfterCross startCond,
            List<(TwoDPoint, CondAfterCross)>? ptsAndCond, List<IBlockShape> temp, CondAfterCross endCond);

        bool CheckAfter(IBlockShape another);

        bool CheckBefore(IBlockShape another);
    }


    public enum CondAfterCross
    {
        ToIn,
        ToOut,
        MaybeOutToIn
    }
}