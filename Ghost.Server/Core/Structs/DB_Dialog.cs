using Ghost.Server.Core.Classes;
using System.Collections.Generic;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Dialog
    {
        public ushort ID;
        public SortedDictionary<short, DialogEntry> Entries;
        public DB_Dialog(ushort id)
        {
            ID = id;
            Entries = new SortedDictionary<short, DialogEntry>();
        }
    }
}