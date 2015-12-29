using System.Collections.Generic;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Spell
    {
        public int ID;
        public SortedDictionary<byte, DB_SpellEffect> Effects;
        public DB_Spell(int id)
        {
            ID = id;
            Effects = new SortedDictionary<byte, DB_SpellEffect>();
        }
    }
}