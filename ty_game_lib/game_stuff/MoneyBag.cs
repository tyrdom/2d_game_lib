using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class MoneyBag
    {
        private Dictionary<int, GameItem> GameItems { get; }
    }

    internal class GameItem
    {
    }

    readonly struct Cost
    {
        private int ItemId { get; }
        private uint Num { get; }
    }

    readonly struct Gain
    {
        private int ItemId { get; }
        private uint Num { get; }
    }

    public class DropBox
    {
    }
}