using Ghost.Server.Utilities;
using System;
using System.Collections.Generic;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Item
    {
        public int ID;
        public int Price;
        public byte Level;
        public string Name;
        public ushort Stack;
        public ItemSlot Slot;
        public ItemFlags Flags;
        public List<Tuple<Stats, int>> Stats;
        public DB_Item(int id, string name, byte flags, byte level, ushort stack, int price, int slot)
        {
            ID = id;
            Name = name;
            Level = level;
            Stack = stack;
            Price = price;
            Slot = (ItemSlot)slot;
            Flags = (ItemFlags)flags;
            if ((Flags & ItemFlags.Stats) > 0)
                Stats = new List<Tuple<Stats, int>>();
            else Stats = null;
        }
    }
}