using Ghost.Server.Core.Classes;
using System.Collections.Generic;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Movement
    {
        public readonly ushort Id;

        public readonly SortedDictionary<ushort, MovementEntry> Entries;

        public DB_Movement(ushort id)
        {
            Id = id;
            Entries = new SortedDictionary<ushort, MovementEntry>();
        }
    }
}