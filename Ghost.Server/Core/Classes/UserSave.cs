using ProtoBuf;
using System.Collections.Generic;
using System.IO;

namespace Ghost.Server.Core.Classes
{
    [ProtoContract]
    public class UserSave
    {
        [ProtoMember(1)]
        public Dictionary<int, byte> Friends;
        public UserSave()
        {
            Friends = new Dictionary<int, byte>();
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