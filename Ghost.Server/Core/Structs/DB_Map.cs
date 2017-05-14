using Ghost.Server.Utilities;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Map
    {
        public readonly int Id;

        public readonly string Name;

        public readonly MapFlags Flags;

        public DB_Map(int id, string name, byte flags)
        {
            Id = id;
            Name = name;
            Flags = (MapFlags)flags;
        }
    }
}