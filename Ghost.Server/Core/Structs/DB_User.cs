using Ghost.Server.Utilities;

namespace Ghost.Server.Core.Structs
{
    public struct DB_User
    {
        public static readonly DB_User Empty = new DB_User(-1, null, null, 0, null);

        public readonly int Id;

        public readonly string Name;

        public readonly string Hash;

        public readonly string Session;

        public readonly AccessLevel Access;

        public bool IsEmpty => Id == Empty.Id;

        public DB_User(int id, string name, string hash, byte access, string session)
        {
            Id = id;
            Name = name;
            Hash = hash;    
            Session = session;
            Access = (AccessLevel)access;
        }
    }
}