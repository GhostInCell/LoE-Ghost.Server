using Ghost.Server.Utilities;
using PNet;
using ProtoBuf;

namespace Ghost.Server.Core.Classes
{
    [ProtoContract]
    public class Item : INetSerializable
    {
        public const int EmptyID = int.MinValue;

        [ProtoMember(1)]
        public int Id;
        [ProtoMember(2)]
        public uint Color;
        [ProtoMember(3)]
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
                return IsEmpty ? 4 : 9 + Sockets.Length * 4;
            }
        }

        public Item()
        {
            Id = EmptyID;
            Color = uint.MaxValue;
            Sockets = ArrayEx.Empty<int>();
        }

        public Item(int id)
            : this()
        {
            Id = id;
        }

        public void OnSerialize(NetMessage message)
        {
            message.Write(Id);
            if (!IsEmpty)
            {
                message.Write(Color);
                message.Write((byte)Sockets.Length);
                foreach (var socket in Sockets)
                    message.Write(socket);
            }
        }

        public void OnDeserialize(NetMessage message)
        {
            Id = message.ReadInt32();
            if (!IsEmpty)
            {
                Color = message.ReadUInt32();
                Sockets = new int[message.ReadByte()];
                for (int index = 0; index < Sockets.Length; index++)
                    Sockets[index] = message.ReadInt32();
            }
        }
    }

    [ProtoContract]
    public class InventorySlot : INetSerializable
    {
        [ProtoMember(1)]
        public int Amount;
        [ProtoMember(2)]
        public Item Item;

        public bool IsEmpty
        {
            get
            {
                return Item.IsEmpty;
            }
        }

        public int AllocSize
        {
            get
            {
                return 6 + Item.AllocSize;
            }
        }

        public InventorySlot()
        {
            Item = new Item();
        }

        public InventorySlot(int itemId, int amount)
        {
            Item = new Item(itemId);
            Amount = amount;
        }

        public void OnSerialize(NetMessage message)
        {
            Item.OnSerialize(message);
            if (!Item.IsEmpty)
                message.Write((ushort)Amount);
        }

        public void OnDeserialize(NetMessage message)
        {
            Item.OnDeserialize(message);
            if (!Item.IsEmpty)
                Amount = message.ReadUInt16();
        }
    }
}