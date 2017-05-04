using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using PNet;
using System;
using System.Collections.Generic;

namespace Ghost.Server.Utilities
{
    public struct SER_RoomInfo : INetSerializable
    {
        private PNetS.Room[] m_data;
        public int AllocSize
        {
            get
            {
                return 4 + 20 * m_data?.Length ?? 0;
            }
        }
        public SER_RoomInfo(PNetS.Room[] data)
        {
            m_data = data;
        }
        public void OnSerialize(NetMessage message)
        {
            int length = m_data.Length;
            message.Write(length);
            for (int i = 0; i < length; i++)
            {
                message.Write(m_data[i].Guid);
                message.Write((ushort)m_data[i].PlayerCount);
                message.Write((ushort)m_data[i].MaxPlayers);
            }

        }
        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }
    public struct SER_Shop : INetSerializable
    {
        private string m_name;
        private List<int> m_data;

        public int AllocSize
        {
            get
            {
                return 5 + m_data.Count * 8 + m_name.Length * 2;
            }
        }

        public SER_Shop(List<int> data, string name)
        {
            m_data = data;
            m_name = name;
        }

        public void OnSerialize(NetMessage message)
        {
            int length = m_data.Count;
            message.Write(length);
            for (int i = 0; i < length; i++)
            {
                message.Write(m_data[i]);
                message.Write(1);
            }
            message.Write(m_name);
        }

        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }

    public struct SER_Stats : INetSerializable
    {
        private Dictionary<Stats, StatValue> m_data;

        public SER_Stats(Dictionary<Stats, StatValue> data)
        {
            if (data == null) throw new ArgumentNullException();
            m_data = data;
        }

        public int AllocSize
        {
            get
            {
                return 4 + m_data.Count * 16;
            }
        }

        public void OnSerialize(NetMessage message)
        {
            message.Write(m_data.Count);
            foreach (var item in m_data)
            {
                message.Write((uint)item.Key);
                message.Write((uint)item.Value.Current);
                message.Write((uint)item.Value.Max);
                message.Write((uint)item.Value.Min);
            }
        }

        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }

    public struct SER_Wears : INetSerializable
    {
        private Dictionary<int, InventoryItem> m_data;

        public int AllocSize
        {
            get
            {
                return 8 + m_data.Count * 32;
            }
        }

        public SER_Wears(Dictionary<int, InventoryItem> data)
        {
            m_data = data;
        }

        public void OnSerialize(NetMessage message)
        {
            InventoryItem item; WearablePosition wSlots = WearablePosition.None; DB_Item dbItem;
            message.Write(Constants.MaxWornItems);
            for (int index = 0, count = m_data.Count; index < Constants.MaxWornItems; index++)
            {
                if (count > 0)
                {
                    if (m_data.TryGetValue(index, out item) && DataMgr.Select(item.Id, out dbItem))
                    {
                        count--;
                        item.OnSerialize(message);
                        wSlots |= dbItem.Slot;
                        continue;
                    }
                }
                message.Write(InventoryItem.EmptyID);
            }
            message.Write((int)wSlots);
        }

        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }

    public struct SER_Skills : INetSerializable
    {
        private readonly CharData _data;

        public int AllocSize
        {
            get
            {
                return 5 + _data.Skills.Count * 8;
            }
        }

        public SER_Skills(CharData data)
        {
            _data = data;
        }

        public void OnSerialize(NetMessage message)
        {
            message.Write((byte)0);
            message.Write(_data.Skills.Count);
            foreach (var item in _data.Skills)
            {
                message.Write(item.Key);
                message.Write(item.Value);
            }
        }
        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }

    public struct SER_Talents : INetSerializable
    {
        private CharData m_data;

        public int AllocSize
        {
            get
            {
                return 4 + m_data.Talents.Count * 16;
            }
        }

        public SER_Talents(CharData data)
        {
            m_data = data;
        }

        public void OnSerialize(NetMessage message)
        {
            message.Write(m_data.Talents.Count);
            foreach (var item in m_data.Talents)
            {
                message.Write((uint)item.Key);
                message.Write(item.Value.Exp);
                message.Write((uint)item.Value.Level);
                message.Write((uint)item.Value.Points);
                if (item.Value.Level == 0)
                {
                    var exp = CharsMgr.GetExpForLevel(item.Value.Level+ 1);
                    message.Write(exp);
                    message.Write(exp);
                    message.Write(0u);
                }
                else if (item.Value.Level == CharsMgr.MaxLevel)
                {
                    var exp = CharsMgr.GetExpForLevel(CharsMgr.MaxLevel);
                    message.Write(exp);
                    message.Write(0u);
                    message.Write(exp);
                }
                else
                {
                    var exp = CharsMgr.GetExpForLevel(item.Value.Level + 1);
                    message.Write(item.Value.Exp);
                    message.Write(exp);
                    message.Write(0);
                }
            }
        }

        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }

    public struct SER_Inventory : INetSerializable
    {
        private readonly CharData m_data;

        public int AllocSize
        {
            get
            {
                return 4 + m_data.Items.Count * 32;
            }
        }

        public SER_Inventory(CharData data)
        {
            m_data = data;
        }

        public void OnSerialize(NetMessage message)
        {
            var items = m_data.Items;
            InventorySlot slot;
            message.Write(m_data.InventorySlots);
            for (int index = 0, count = items.Count; index < m_data.InventorySlots; index++)
            {
                if (count > 0)
                {
                    if (items.TryGetValue(index, out slot))
                    {
                        count--;
                        slot.OnSerialize(message);
                        continue;
                    }
                }
                message.Write(InventoryItem.EmptyID);
            }
        }

        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }
}