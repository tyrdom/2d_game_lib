#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;

namespace collision_and_rigid
{
    public abstract class QSpace
    {
        public virtual Quad? TheQuad { get; set; }
        public QSpaceBranch? Father { get; set; }
        public abstract Zone Zone { get; set; }

        public abstract HashSet<AabbBoxShape> AabbPackBoxShapes { get; set; }

        public abstract void AddIdPoint(HashSet<AabbBoxShape> idPointShapes, int limit);

        public abstract void MoveIdPoint(Dictionary<int, ITwoDTwoP> gidToMove, int limit);


        public abstract TwoDPoint? GetSlidePoint(TwoDVectorLine line, bool isPush, bool safe = true);

        public abstract void InsertBox(AabbBoxShape boxShape);
        public abstract void Remove(AabbBoxShape boxShape);
        public abstract IEnumerable<AabbBoxShape> TouchBy(AabbBoxShape boxShape);
        public abstract bool LineIsCross(TwoDVectorLine line);


        public abstract QSpace TryCovToLimitQSpace(int limit);
        public abstract (int, AabbBoxShape?) TouchWithARightShootPoint(TwoDPoint p);
        public abstract string OutZones();
        public abstract int FastTouchWithARightShootPoint(TwoDPoint p);

        public abstract void ForeachDoWithOutMove<T>(Action<IIdPointShape, T> doWithIIdPointShape, T t);

        public Dictionary<int, TU> MapToDicGidToSth<TU, T>(Func<IIdPointShape, T, TU> funcWithIIdPtsShape,
            T t)
        {
            var dicIntToTu = new Dictionary<int, TU>();

            void Act(IIdPointShape id, T tt)
            {
                var withIIdPtsShape = funcWithIIdPtsShape(id, tt);
                if (withIIdPtsShape == null) return;
                var i = id.GetId();

                dicIntToTu[i] = withIIdPtsShape;
            }

            ForeachDoWithOutMove(Act, t);
            return dicIntToTu;
        }

        public IEnumerable<IIdPointShape> FilterToGIdPsList<T>(Func<IIdPointShape, T, bool> funcWithIIdPtsShape,
            T t)
        {
            var dicIntToTu = new HashSet<IIdPointShape>();

            void Act(IIdPointShape id, T tt)
            {
                var withIIdPtsShape = funcWithIIdPtsShape(id, tt);

                if (withIIdPtsShape) dicIntToTu.Add(id);
            }

            ForeachDoWithOutMove(Act, t);
            return dicIntToTu;
        }

        public IEnumerable<T> MapToIEnum<T>(Func<IIdPointShape, T> funcWithIIdPtsShape
        )
        {
            var dicIntToTu = new HashSet<T>();

            void Act(IIdPointShape id, bool a)
            {
                var withIIdPtsShape = funcWithIIdPtsShape(id);
                dicIntToTu.Add(withIIdPtsShape);
            }
            ForeachDoWithOutMove(Act, true);
            return dicIntToTu;
        }
    }


    public enum Quad
    {
        One,
        Two,
        Three,
        Four
    }
}