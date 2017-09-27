using Ghost.Server.Utilities;
using System;
using System.Net;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Ban
    {
        public static readonly DB_Ban Empty = new DB_Ban(-1, 0, null, 0, 0, DateTime.MinValue, DateTime.MaxValue, 0);

        public readonly int Id;
        public readonly int BanBy;
        public readonly string Reason;
        public readonly IPAddress Ip;
        public readonly int User;
        public readonly DateTime Start;
        public readonly DateTime End;
        public readonly BanType Type;

        public bool IsEmpty => Id == Empty.Id;

        public DB_Ban(int id, int by, string reason, long ip, int user, DateTime start, DateTime end, byte type)
        {
            Id = id;
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
