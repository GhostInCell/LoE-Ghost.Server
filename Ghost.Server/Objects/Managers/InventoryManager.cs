using Ghost.Server.Utilities;
using PNet;
using PNetR;
using ProtoBuf;

namespace Ghost.Server.Objects.Managers
{
    [ProtoContract]
    public class InventorySlot : INetSerializable
    {
        [ProtoMember(1)]
        public int Amount;
        [ProtoMember(2)]
        public InventoryItem Item;

        public bool IsEmpty
        {
            get
            {
                return Item.IsEmpty || Amount <= 0;
            }
        }

        public int AllocSize
        {
            get
            {
                return 2 + Item.AllocSize;
            }
        }

        public InventorySlot()
        {
            Item = new InventoryItem();
        }

        public InventorySlot(int itemId, int amount)
        {
            Amount = amount;
            Item = new InventoryItem(itemId);
        }

        public InventorySlot(InventoryItem item, int amount)
        {
            Item = item;
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

    [ProtoContract]
    public class InventoryItem : INetSerializable
    {
        public const int EmptyID = int.MinValue;
        public const uint DefaultColor = uint.MaxValue;

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
                return Id == EmptyID ? 4 : 9 + Sockets.Length * 4;
            }
        }

        public InventoryItem()
        {
            Id = EmptyID;
            Color = DefaultColor;
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
                message.Write(Color);
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
                Color = message.ReadUInt32();
                Sockets = new int[message.ReadByte()];
                for (int index = 0; index < Sockets.Length; index++)
                    Sockets[index] = message.ReadInt32();
            }
        }
    }

    [NetComponent(7)]
    public class InventoryManager : NetworkManager<PlayerObject>
    {
        private struct WornSlot : INetSerializable
        {
            public byte Value;

            public int Index
            {
                get
                {
                    return Value - 1;
                }
            }

            public int AllocSize
            {
                get
                {
                    return 1;
                }
            }

            public bool IsWearable
            {
                get
                {
                    return Value > 0 && Value <= 32;
                }
            }

            public WearablePosition Position
            {
                get
                {
                    return Value.ToWearablePosition();
                }
            }

            public void OnSerialize(NetMessage message)
            {
                message.Write(Value);
            }

            public void OnDeserialize(NetMessage message)
            {
                Value = message.ReadByte();
            }

            public override string ToString()
            {
                return Position.ToString();
            }
        }

        private InventoryItem[] m_wears;
        private InventorySlot[] m_items;

        public InventoryManager()
            : base()
        {
            m_wears = ArrayEx.Empty<InventoryItem>();
            m_items = ArrayEx.Empty<InventorySlot>();
        }
        #region RPC Handlers
        [Rpc(4, false)]//WornItems
        private void RPC_004(NetMessage arg1, NetMessageInfo arg2)
        {

        }

        [Rpc(6, false)]//AddItem
        private void RPC_006(NetMessage arg1, NetMessageInfo arg2)
        {

        }

        [Rpc(7, false)]//DeleteItem
        private void RPC_007(NetMessage arg1, NetMessageInfo arg2)
        {

        }

        [Rpc(8, false)]//WearItem
        private void RPC_008(NetMessage message, NetMessageInfo info)
        {
        }

        [Rpc(9, false)]//UnwearItem
        private void RPC_009(NetMessage message, NetMessageInfo info)
        {

        }

        [Rpc(11, false)]//SellItem
        private void RPC_011(NetMessage message, NetMessageInfo info)
        {

        }

        [Rpc(12, false)]//UseItem
        private void RPC_012(NetMessage message, NetMessageInfo info)
        {

        }

        [Rpc(13, false)]//CombineItems
        private void RPC_013(NetMessage message, NetMessageInfo info)
        {

        }

        [Rpc(14, false)]//ColorItem
        private void RPC_014(NetMessage message, NetMessageInfo info)
        {

        }

        [Rpc(15, false)]//SocketItem
        private void RPC_015(NetMessage message, NetMessageInfo info)
        {

        }

        [Rpc(20, false)]//MoveSlotToSlot
        private void RPC_020(NetMessage message, NetMessageInfo info)
        {

        }
        #endregion
        #region Overridden Methods
        protected override void OnViewCreated()
        {
            base.OnViewCreated();
            m_view.SubscribeMarkedRpcsOnComponent(this);
        }
        #endregion
    }
}