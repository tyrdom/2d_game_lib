#nullable enable
using System;
using System.Collections.Generic;

namespace collision_and_rigid
{
    public interface IQSpace
    {
        public Quad? TheQuad { get; set; }
        public QSpaceBranch? Father { get; set; }
        public Zone Zone { get; set; }

        public HashSet<AabbBoxShape> AabbPackBoxShapes { get; set; }

        public void AddIdPoint(HashSet<AabbBoxShape> idPointShapes, int limit);

        public void MoveIdPoint(Dictionary<int, ITwoDTwoP> gidToMove, int limit);


        public TwoDPoint? GetSlidePoint(TwoDVectorLine line, bool isPush, bool safe = true);

        public void InsertBox(AabbBoxShape boxShape);
        public void Remove(AabbBoxShape boxShape);
        public IEnumerable<AabbBoxShape> TouchBy(AabbBoxShape boxShape);
        public bool LineIsBlockSight(TwoDVectorLine line);
        
        public IQSpace TryCovToLimitQSpace(int limit);
        public (int, AabbBoxShape?) TouchWithARightShootPoint(TwoDPoint p);
        public string OutZones();
        public int FastTouchWithARightShootPoint(TwoDPoint p);

        public void ForeachDoWithOutMove<T>(Action<IIdPointShape, T> doWithIIdPointShape, T t);

        public Dictionary<int, TU> MapToDicGidToSth<TU, T>(Func<IIdPointShape, T, TU> funcWithIIdPtsShape,
            T t);

        public IEnumerable<IIdPointShape> FilterToGIdPsList<T>(Func<IIdPointShape, T, bool> funcWithIIdPtsShape,
            T t);

        public IEnumerable<T> MapToIEnum<T>(Func<IIdPointShape, T> funcWithIIdPtsShape
        );
    }


    public enum Quad
    {
        One,
        Two,
        Three,
        Four
    }
}