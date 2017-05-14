using System.Collections.Generic;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Spell
    {
        public readonly int Id;

        public readonly SortedDictionary<byte, DB_SpellEffect> Effects;

        public DB_Spell(int id)
        {
            Id = id;
            Effects = new SortedDictionary<byte, DB_SpellEffect>();
        }
    }
}