using Ghost.Server.Core.Classes;
using Ghost.Server.Utilities;
using System.Collections.Generic;

namespace Ghost.Server.Core.Structs
{
    public struct DB_NPC
    {
        public int ID;
        public short Level;
        public ushort Dialog;
        public PonyData Pony;
        public NPCFlags Flags;
        public List<int> Items;
        public List<int> Wears;
        public ushort Movement;
        public byte DialogIndex;
        public DB_NPC(int id, byte flags, short level, ushort dialog, byte index, ushort movement, PonyData pony)
        {
            ID = id;
            Pony = pony;
            Level = level;
            Dialog = dialog;
            DialogIndex = index;
            Movement = movement;
            Flags = (NPCFlags)flags;
            if ((Flags & NPCFlags.Trader) > 0)
                Items = new List<int>();
            else Items = null;
            if ((Flags & NPCFlags.Wears) > 0)
                Wears = new List<int>();
            else Wears = null;
        }
    }
}