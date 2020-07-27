using System;
using collision_and_rigid;
using Monad;


namespace game_stuff
{
    [Serializable]
    public class Operate
    {
        // public OpAction? Action;
        // public TwoDVector? Move;
        public TwoDVector? Aim;

        public Either<OpAction, TwoDVector>? ActOrMove;


        public Operate(TwoDVector aim, Either<OpAction, TwoDVector>? actOrMove)
        {
            Aim = aim;
            ActOrMove = actOrMove;
        }

        public OpAction? GetAction()
        {
            if (ActOrMove == null)
            {
                return null;
            }

            return ActOrMove.IsLeft() ? ActOrMove.Left() : (OpAction?) null;
        }

        public TwoDVector? GetMove()
        {
            if (ActOrMove == null)
            {
                return null;
            }

            return ActOrMove.IsRight() ? ActOrMove.Right() : null;
        }
    }

    public enum OpAction
    {
        Op1,
        Op2,
        Op3,
        Switch,
        Pick //  far away TODO
    }
}