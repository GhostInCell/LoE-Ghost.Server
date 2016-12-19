using Ghost.Server.Utilities;
using PNet;
using ProtoBuf;

namespace Ghost.Server.Core.Classes
{
    [ProtoContract]
    public class InventoryItem : INetSerializable
    {
        public const int EmptyID = int.MinValue;
        public const uint DefaultColor = uint.MaxValue;

        [ProtoMember(1)]
        public int Id;
        [ProtoMember(2)]
        public uint Color01;
        [ProtoMember(3)]
        public uint Color02;
        [ProtoMember(4)]
        public int[] Sockets;

        public bool IsEmpty
        {
            get
            {
                return Id == EmptyID;
            }
        }

        public int AllocSize
        {
            get
            {
                return Id == EmptyID ? 4 : 9 + Sockets.Length * 4;
            }
        }

        public InventoryItem()
        {
            Id = EmptyID;
            Color01 = DefaultColor;
            Sockets = ArrayEx.Empty<int>();
        }

        public InventoryItem(int id)
            : this()
        {
            Id = id;
        }

        public void OnSerialize(NetMessage message)
        {
            message.Write(Id);
            if (Id != EmptyID)
            {
                message.Write(Color01);
                message.Write(Color02);
                message.Write((byte)Sockets.Length);
                foreach (var socket in Sockets)
                    message.Write(socket);
            }
        }

        public void OnDeserialize(NetMessage message)
        {
            Id = message.ReadInt32();
            if (Id != EmptyID)
            {
                Color01 = message.ReadUInt32();
                Color02 = message.ReadUInt32();
                Sockets = new int[message.ReadByte()];
                for (int index = 0; index < Sockets.Length; index++)
                    Sockets[index] = message.ReadInt32();
            }
        }
    }
}