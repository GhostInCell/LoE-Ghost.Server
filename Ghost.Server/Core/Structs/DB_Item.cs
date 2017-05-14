using Ghost.Server.Utilities;
using System.Collections.Generic;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Item
    {
        public readonly int ID;
        public readonly int Price;
        public readonly uint Color;
        public readonly byte Level;
        public readonly string Name;
        public readonly byte Sockets;
        public readonly ushort Stack;
        public readonly ItemFlags Flags;
        public readonly WearablePosition Slot;
        public readonly List<(Stats, int)> Stats;

        public DB_Item(int id, string name, byte flags, byte level, byte rlevel, ushort stack, byte sockets, int price, uint slot, uint color)
        {
            ID = id;
            Name = name;
            Level = level;
            Stack = stack;
            Price = price;
            Color = color;
            Sockets = sockets;
            Flags = (ItemFlags)flags;
            Slot = (WearablePosition)slot;
            Stats = new List<(Stats, int)>();
        }
    }
}