using System;
using System.Collections.Generic;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Loot
    {
        public int ID;
        public List<Tuple<int, int, int, float, int, int>> Loot;
        public DB_Loot(int id)
        {
            ID = id;
            Loot = new List<Tuple<int, int, int, float, int, int>>();
        }
    }
}