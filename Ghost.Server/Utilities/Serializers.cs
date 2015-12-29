using Ghost.Server.Core.Classes;
using PNet;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly List<int> _data;
        public int AllocSize
        {
            get
            {
                return 4 + _data.Count * 8;
            }
        }
        public SER_Shop(List<int> data)
        {
            _data = data;
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
                return 4 + _data.Count * 12;
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
            }
        }
        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }
    public class SER_Wears : INetSerializable
    {
        private readonly CharData _data;
        public int AllocSize
        {
            get
            {
                return 2 + _data.Wears.Count * 5;
            }
        }
        public SER_Wears(CharData data)
        {
            _data = data;
        }
        public void OnSerialize(NetMessage message)
        {
            message.Write(Constants.MaxWornItems);
            message.Write((byte)_data.Wears.Count);
            foreach (var item in _data.Wears)
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
        private readonly List<Tuple<int, int>> _data;
        public int AllocSize
        {
            get
            {
                return 4 + _data.Count * 8;
            }
        }
        public SER_Trade(List<Tuple<int, int>> data)
        {
            _data = data;
        }
        public void OnSerialize(NetMessage message)
        {
            int length = _data.Count;
            message.Write(length);
            for (int i = 0; i < length; i++)
            {
                message.Write(_data[i].Item1);
                message.Write(_data[i].Item2);
            }

        }
        public void OnDeserialize(NetMessage message)
        {
            _data.Clear();
            int length = message.ReadInt32();
            for (int i = 0; i < length; i++)
                _data.Add(new Tuple<int, int>(message.ReadInt32(), message.ReadInt32()));
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