using Ghost.Server.Utilities;
using PNet;

namespace Ghost.Server.Core.Classes
{
    public class UserData : INetSerializable
    {
        public int ID;
        public int Map;
        public int Char;
        public string Name;
        public ushort Spawn;
        public AccessLevel Access;
        public bool IsEmpty
        {
            get
            {
                return ID == -1;
            }
        }
        public int AllocSize
        {
            get { return 16 + (Name?.Length ?? 0) * 2; }
        }
        public UserData()
        {
            ID = -1;
        }
        public void OnSerialize(NetMessage message)
        {
            message.Write(ID);
            message.Write(Map);
            message.Write(Char);
            message.Write(Spawn);
            message.Write(Name);
            message.Write((byte)Access);
        }
        public void OnDeserialize(NetMessage message)
        {
            ID = message.ReadInt32();
            Map = message.ReadInt32();
            Char = message.ReadInt32();
            Spawn = message.ReadUInt16();
            Name = message.ReadString();
            Access = (AccessLevel)message.ReadByte();
        }
    }
}