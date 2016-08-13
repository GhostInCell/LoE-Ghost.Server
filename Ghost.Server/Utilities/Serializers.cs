using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using PNet;
using System;
using System.Collections.Generic;

namespace Ghost.Server.Utilities
{
    public class SER_RoomInfo : INetSerializable
    {
        private readonly PNetS.Room[] _data;
        public int AllocSize
        {
            get
            {
                return 4 + 20 * _data?.Length ?? 0;
            }
        }
        public SER_RoomInfo(PNetS.Room[] data)
        {
            _data = data;
        }
        public void OnSerialize(NetMessage message)
        {
            int length = _data.Length;
            message.Write(length);
            for (int i = 0; i < length; i++)
            {
                message.Write(_data[i].Guid);
                message.Write((ushort)_data[i].PlayerCount);
                message.Write((ushort)_data[i].MaxPlayers);
            }

        }
        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }
    public class SER_Shop : INetSerializable
    {
        private readonly string _name;
        private readonly List<int> _data;
        public int AllocSize
        {
            get
            {
                return 5 + _data.Count * 8 + (_name?.Length ?? 0) * 2;
            }
        }
        public SER_Shop(List<int> data, string name)
        {
            _data = data;
            _name = name;
        }
        public void OnSerialize(NetMessage message)
        {
            int length = _data.Count;
            message.Write(length);
            for (int i = 0; i < length; i++)
            {
                message.Write(_data[i]);
                message.Write(1);
            }
            message.Write(_name);
        }
        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }
    public class SER_Stats : INetSerializable
    {
        private readonly Dictionary<Stats, StatValue> _data;
        public SER_Stats(Dictionary<Stats, StatValue> data)
        {
            if (data == null) throw new ArgumentNullException();
            _data = data;
        }
        public int AllocSize
        {
            get
            {
                return 4 + _data.Count * 16;
            }
        }
        public void OnSerialize(NetMessage message)
        {
            message.Write(_data.Count);
            foreach (var item in _data)
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
    public class SER_Wears : INetSerializable
    {
        private readonly Dictionary<int, InventoryItem> m_data;
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
    public class SER_Skills : INetSerializable
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
    public class SER_Talents : INetSerializable
    {
        private readonly CharData _data;
        public int AllocSize
        {
            get
            {
                return 4 + _data.Talents.Count * 16;
            }
        }
        public SER_Talents(CharData data)
        {
            _data = data;
        }
        public void OnSerialize(NetMessage message)
        {
            message.Write(_data.Talents.Count);
            foreach (var item in _data.Talents)
            {
                message.Write((uint)item.Key);
                message.Write(0u);
                message.Write((uint)item.Value.Level);
                message.Write((uint)item.Value.Points);
                message.Write(0u);
                message.Write(0u);
                message.Write(0u);
                //if (item.Value.Item2 == 0)
                //{
                //    var exp = CharsMgr.GetExpForLevel(item.Value.Item2 + 1);
                //    message.Write(exp);
                //    message.Write(exp);
                //    message.Write(0u);
                //}
                //else if (item.Value.Item2 == CharsMgr.MaxLevel)
                //{
                //    var exp = CharsMgr.GetExpForLevel(CharsMgr.MaxLevel);
                //    message.Write(exp);
                //    message.Write(0u);
                //    message.Write(exp);
                //}
                //else
                //{
                //    var exp = CharsMgr.GetExpForLevel(item.Value.Item2 + 1);
                //    message.Write(exp - item.Value.Item1);
                //    message.Write(exp);
                //    message.Write(CharsMgr.GetExpForLevel(item.Value.Item2));
                //}
            }
        }
        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }
    public class SER_Inventory : INetSerializable
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