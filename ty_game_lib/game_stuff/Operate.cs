using System;
using System.Collections.Generic;
using collision_and_rigid;
using FSharpx.DataStructures;
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
            return ActOrMove.IsLeft() ? ActOrMove.Left() : (OpAction?) null;
        }
    }

    public enum OpAction
    {
        Op1,
        Op2,
        Op3,
        Switch
    }
}