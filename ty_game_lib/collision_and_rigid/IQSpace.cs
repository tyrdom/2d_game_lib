using System;
using System.Collections.Generic;

namespace collision_and_rigid
{
    public interface IQSpace
    {
        public int Count();
        public Quad? TheQuad { get; set; }
        public QSpaceBranch? Father { get; set; }
        public Zone Zone { get; }

        public HashSet<IAaBbBox> AaBbPackBox { get; }

        public void AddIdPoint(HashSet<IdPointBox> idPointShapes, int limit);

        public void MoveIdPoint(Dictionary<int, ITwoDTwoP> gidToMove, int limit);


        public void RemoveIdPoint(IdPointBox idPointBox);

        public TwoDPoint? GetSlidePoint(TwoDVectorLine line, bool safe = true);

        public void InsertBlockBox(BlockBox box);
        public void RemoveBlockBox(BlockBox box);
        public IEnumerable<BlockBox> TouchBy(BlockBox box);
        public bool LineIsBlockSight(TwoDVectorLine line);

        public IQSpace AddSingleAaBbBox(IAaBbBox aaBbBox, int limit);

        public bool RemoveSingleAaBbBox(IAaBbBox aaBbBox);

        public IAaBbBox? InteractiveFirstSingleBox(TwoDPoint pos);
        public (int, BlockBox?) TouchWithARightShootPoint(TwoDPoint p);
        public string OutZones();
        public int FastTouchWithARightShootPoint(TwoDPoint p);

        public void ForeachDoWithOutMove<T>(Action<IIdPointShape, T> doWithIIdPointShape, T t);
        public void ForeachDoWithOutMove<T>(Action<IIdPointShape, T> doWithIIdPointShape, T t, Zone zone);

        public Dictionary<int, TU> MapToDicGidToSth<TU, T>(Func<IIdPointShape, T, TU> funcWithIIdPtsShape,
            T t);

        public IEnumerable<IIdPointShape> FilterToGIdPsList<T>(Func<IIdPointShape, T, bool> funcWithIIdPtsShape,
            T t);

        public IEnumerable<IIdPointShape> FilterToGIdPsList<T>(Func<IIdPointShape, T, bool> funcWithIIdPtsShape,
            T t, Zone zone);

        public IEnumerable<T> MapToIEnum<T>(Func<IIdPointShape, T> funcWithIIdPtsShape
        );

        public HashSet<IBlockShape> GetAllIBlocks();


        public AreaBox? PointInWhichArea(TwoDPoint pt);
    }
}