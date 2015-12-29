using Ghost.Server.Utilities;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Map
    {
        public int ID;
        public string Name;
        public MapFlags Flags;
        public DB_Map(int id, string name, byte flags)
        {
            ID = id;
            Name = name;
            Flags = (MapFlags)flags;
        }
    }
}