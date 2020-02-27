using System.Collections.Generic;
using System.Dynamic;

namespace collision_and_rigid
{
    public interface IShape
    {
        AabbBoxShape CovToAabbPackBox();
        
        int TouchByRightShootPointInAAbbBox(TwoDPoint p);

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
    
    
    public interface IIdPointShape:IShape
    {
        int GetId();
        TwoDPoint Move(TwoDVector vector);
        TwoDPoint GetAchor();
    }

    public interface IBlockShape
    {
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