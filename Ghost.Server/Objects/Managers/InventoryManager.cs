using Ghost.Server.Core.Classes;
using Ghost.Server.Utilities;
using PNet;
using PNetR;

namespace Ghost.Server.Objects.Managers
{
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
                return $"{(Value - 1):00}[{Value.ToWearablePosition()}]";
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
        private void RPC_004(NetMessage message, NetMessageInfo info)
        {

        }

        [Rpc(6, false)]//AddItem
        private void RPC_006(NetMessage message, NetMessageInfo info)
        {

        }

        [Rpc(7, false)]//DeleteItem
        private void RPC_007(NetMessage message, NetMessageInfo info)
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