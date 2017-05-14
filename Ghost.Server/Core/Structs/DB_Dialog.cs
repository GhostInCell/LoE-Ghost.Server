using Ghost.Server.Core.Classes;
using System.Collections.Generic;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Dialog
    {
        public readonly ushort Id;

        public readonly SortedDictionary<short, DialogEntry> Entries;

        public DB_Dialog(ushort id)
        {
            Id = id;
            Entries = new SortedDictionary<short, DialogEntry>();
        }
    }
}