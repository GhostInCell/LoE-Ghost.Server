using PNet;
using ProtoBuf;

namespace Ghost.Server.Core.Classes
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
}