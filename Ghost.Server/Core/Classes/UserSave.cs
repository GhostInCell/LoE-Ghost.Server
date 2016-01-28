using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ghost.Server.Core.Classes
{
    [ProtoContract]
    public class UserSave
    {
        [ProtoMember(1)]
        public Dictionary<int, Tuple<byte, string, short, DateTime>> Friends;
        public UserSave()
        {
            Friends = new Dictionary<int, Tuple<byte, string, short, DateTime>>();
        }
        public byte[] GetBytes()
        {
            using (var mem = new MemoryStream())
            {
                Serializer.Serialize(mem, this);
                return mem.ToArray();
            }
        }
    }
}