using Ghost.Server.Core.Classes;
using Ghost.Server.Utilities;
using System.Collections.Generic;

namespace Ghost.Server.Core.Structs
{
    public struct DB_NPC
    {
        public int ID;
        public short Level;
        public ushort Script;
        public ushort Dialog;
        public PonyData Pony;
        public NPCFlags Flags;
        public List<int> Items;
        public List<int> Wears;
        public byte DialogIndex;
        public DB_NPC(int id, byte flags, short level, ushort dialog, byte index, ushort script, PonyData pony)
        {
            ID = id;
            Pony = pony;
            Level = level;
            Script = script;
            Dialog = dialog;
            DialogIndex = index;
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