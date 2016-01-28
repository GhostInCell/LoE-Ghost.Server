using Ghost.Server.Core.Classes;
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
        private readonly Dictionary<Stats, StatHelper> _data;
        public SER_Stats(Dictionary<Stats, StatHelper> data)
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
        private readonly Dictionary<byte, int> _data;
        public int AllocSize
        {
            get
            {
                return 2 + _data.Count * 5;
            }
        }
        public SER_Wears(Dictionary<byte, int> data)
        {
            _data = data;
        }
        public void OnSerialize(NetMessage message)
        {
            message.Write(Constants.MaxWornItems);
            message.Write((byte)_data.Count);
            foreach (var item in _data)
            {
                message.Write((byte)(item.Key - 1));
                message.Write(item.Value);
            }
        }
        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }
    public class SER_Trade : INetSerializable
    {
        private readonly Dictionary<int, int> _data;
        public int AllocSize
        {
            get
            {
                return 4 + _data.Count * 8;
            }
        }
        public SER_Trade(Dictionary<int, int> data)
        {
            _data = data;
        }
        public void OnSerialize(NetMessage message)
        {
            int length = _data.Count;
            message.Write(length);
            foreach (var item in _data)
            {
                message.Write(item.Key);
                message.Write(item.Value);
            }
        }
        public void OnDeserialize(NetMessage message)
        {
            int item, nCount, oCount;
            for (int length = message.ReadInt32(); length > 0; length--)
            {
                item = message.ReadInt32();
                nCount = message.ReadInt32();
                if (_data.TryGetValue(item, out oCount))
                    _data[item] = nCount + oCount;
                else _data[item] = nCount;
            }
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
                message.Write(item.Key);
                message.Write(item.Value.Item1);
                message.Write((int)item.Value.Item2);   
                message.Write(0u);
                message.Write(0u);
                message.Write(0u);
            }
        }
        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }
    public class SER_Inventory : INetSerializable
    {
        private readonly CharData _data;
        public int AllocSize
        {
            get
            {
                return 8 + _data.Items.Count * 9 + _data.Wears.Count * 5;
            }
        }
        public SER_Inventory(CharData data)
        {
            _data = data;
        }
        public void OnSerialize(NetMessage message)
        {
            message.Write(_data.InvSlots);
            message.Write((byte)_data.Items.Count);
            foreach (var item in _data.Items)
            {
                message.Write(item.Key);
                message.Write(item.Value.Item1);
                message.Write(item.Value.Item2);
            }
            message.Write(Constants.MaxWornItems);
            message.Write((byte)_data.Wears.Count);
            foreach (var item in _data.Wears)
            {
                message.Write((byte)(item.Key - 1));
                message.Write(item.Value);
            }
            message.Write(_data.Bits);
        }
        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }
}