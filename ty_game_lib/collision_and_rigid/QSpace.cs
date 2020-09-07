#nullable enable
using System;
using System.Collections.Generic;

namespace collision_and_rigid
{
    public interface IQSpace
    {
        public int Count();
        public Quad? TheQuad { get; set; }
        public QSpaceBranch? Father { get; set; }
        public Zone Zone { get; set; }

        public HashSet<IAaBbBox> AabbPackBox { get; set; }

        public void AddIdPoint(HashSet<IdPointBox> idPointShapes, int limit);

        public void MoveIdPoint(Dictionary<int, ITwoDTwoP> gidToMove, int limit);


        public TwoDPoint? GetSlidePoint(TwoDVectorLine line, bool safe = true);

        public void InsertBlockBox(BlockBox box);
        public void Remove(BlockBox box);
        public IEnumerable<BlockBox> TouchBy(BlockBox box);
        public bool LineIsBlockSight(TwoDVectorLine line);

       
        public (int, BlockBox?) TouchWithARightShootPoint(TwoDPoint p);
        public string OutZones();
        public int FastTouchWithARightShootPoint(TwoDPoint p);

        public void ForeachDoWithOutMove<T>(Action<IIdPointShape, T> doWithIIdPointShape, T t);

        public Dictionary<int, TU> MapToDicGidToSth<TU, T>(Func<IIdPointShape, T, TU> funcWithIIdPtsShape,
            T t);

        public IEnumerable<IIdPointShape> FilterToGIdPsList<T>(Func<IIdPointShape, T, bool> funcWithIIdPtsShape,
            T t);

        public IEnumerable<T> MapToIEnum<T>(Func<IIdPointShape, T> funcWithIIdPtsShape
        );

        public HashSet<IBlockShape> GetAllIBlocks();


        public AreaBox? PointInWhichArea(TwoDPoint pt);
    }


    public enum Quad
    {
        One,
        Two,
        Three,
        Four
    }
}