using Ghost.Server.Core.Classes;
using System.Collections.Generic;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Movement
    {
        public ushort ID;
        public SortedDictionary<ushort, MovementEntry> Entries;
        public DB_Movement(ushort id)
        {
            ID = id;
            Entries = new SortedDictionary<ushort, MovementEntry>();
        }
    }
}