using System;
using System.Collections.Generic;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Loot
    {
        public readonly int Id;

        public readonly List<(int, int, int, float, int, int)> Loot;

        public DB_Loot(int id)
        {
            Id = id;
            Loot = new List<(int, int, int, float, int, int)>();
        }
    }
}