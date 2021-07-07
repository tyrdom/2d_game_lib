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

        public string ToString(int level);
        public HashSet<IAaBbBox> AaBbPackBox { get; }

        public void AddIdPointBoxes(HashSet<IdPointBox> idPointBox, int limit, bool needRecord = false);

        public void AddAIdPointBox(IdPointBox idPointBox, int limit, bool needRecord = false);
        public void ReLocateIdBoxInQuadTree(int[] gidToMove, int limit);

        public bool RemoveAIdPointBox(IdPointBox idPointBox);
        public IEnumerable<IdPointBox> RemoveIdPointBoxes(IEnumerable<IdPointBox> idPointBoxes);


        public TwoDPoint? GetSlidePoint(TwoDVectorLine line, bool safe = true);

        public void InsertBlockBox(BlockBox box);
        public void RemoveBlockBox(BlockBox box);
        public IEnumerable<BlockBox> TouchBy(BlockBox box);
        public bool LineIsBlockSight(TwoDVectorLine line);

        public void AddSingleAaBbBox(IAaBbBox aaBbBox, int limit);
        public void AddRangeAabbBoxes(HashSet<IAaBbBox> aaBbBoxes, int limit);
        public bool RemoveSingleAaBbBox(IAaBbBox aaBbBox);

        // public bool RemoveAaBbBoxes(HashSet<IAaBbBox> aaBbBoxes);
        
        public IAaBbBox? InteractiveFirstSingleBox(TwoDPoint pos, Func<IAaBbBox, bool>? filter = null);
        public (int, BlockBox?) TouchWithARightShootPoint(TwoDPoint p);
        public string OutZones();
        public int FastTouchWithARightShootPoint(TwoDPoint p);

        public void ForeachBoxDoWithOutMove<T, TK>(Action<TK, T> action, T t);
        public void ForeachBoxDoWithOutMove<T, TK>(Action<TK, T> action, T t, Zone zone);
        public void ForeachDoWithOutMove<T>(Action<IIdPointShape, T> doWithIIdPointShape, T t);
        public void ForeachDoWithOutMove<T>(Action<IIdPointShape, T> doWithIIdPointShape, T t, Zone zone);

        public Dictionary<int, TU> MapToDicGidToSth<TU, T>(Func<IIdPointShape, T, TU> funcWithIIdPtsShape,
            T t);

        public IEnumerable<TK> FilterToBoxList<TK, T>(Func<TK, T, bool> func,
            T t, Zone zone);

        public IEnumerable<TK> FilterToBoxList<TK, T>(Func<TK, T, bool> func,
            T t);

        public Dictionary<int, T> MapToIDict<T>(Func<IIdPointShape, T> funcWithIIdPtsShape);

        public HashSet<IBlockShape> GetAllIBlocks();


        public AreaBox? PointInWhichArea(TwoDPoint pt);
        void Clear();
        bool NeedCov(int count, int limit);
    }
}