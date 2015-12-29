using Ghost.Server.Utilities;

namespace Ghost.Server.Core.Structs
{
    public struct DB_User
    {
        public static readonly DB_User Empty = new DB_User(-1, null, null, 0, null);
        public int ID;
        public string SID;
        public string Hash;
        public string Name;
        public AccessLevel Access;
        public DB_User(int id, string name, string hash, byte access, string sid)
        {
            SID = sid;
            Access = (AccessLevel)access;
            Hash = hash;
            Name = name;
            ID = id;
        }
    }
}