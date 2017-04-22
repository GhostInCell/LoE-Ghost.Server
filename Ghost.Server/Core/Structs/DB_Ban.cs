using Ghost.Server.Utilities;
using System;
using System.Net;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Ban
    {
        public int ID;
        public int BanBy;
        public string Reason;
        public IPAddress Ip;
        public int User;
        public DateTime Start;
        public DateTime End;
        public BanType Type;

        public DB_Ban(int id, int by, string reason, long ip, int user, DateTime start, DateTime end, byte type)
        {
            ID = id;
            BanBy = by;
            Reason = reason;
            Ip = ip == -1 ? null : new IPAddress(ip);
            User = user;
            Start = start;
            End = end;
            Type = (BanType)type;
        }
    }
}
