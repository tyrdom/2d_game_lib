using System.Globalization;

namespace ty_game_lib
{
    public class QSpaceBranch
    {
        private Either<QSpaceBranch, QSpaceLeaf> LeftUp;
        private Either<QSpaceBranch, QSpaceLeaf> RightUp;
        private Either<QSpaceBranch, QSpaceLeaf> LeftDown;
        private Either<QSpaceBranch, QSpaceLeaf> RightDown;
    }


    public class QSpaceLeaf
    {
        private Shape[] shapes;
    }

    internal class Shape
    {
    }
}