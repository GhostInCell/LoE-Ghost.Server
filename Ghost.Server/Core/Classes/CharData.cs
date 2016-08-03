using Ghost.Server.Utilities;
using PNet;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Ghost.Server.Core.Classes
{
    [ProtoContract]
    public class CharData
    {
        [ProtoMember(1)]
        public int Herd;
        [ProtoMember(2)]
        public int Bits;
        [ProtoMember(3)]
        public Vector3 Position;
        [ProtoMember(4)]
        public Vector3 Rotation;
        [ProtoMember(5)]
        public int InventorySlots;
        [ProtoMember(6)]
        public Dictionary<int, Item> Wears;
        [ProtoMember(7)]
        public Dictionary<int, int> Skills;
        [ProtoMember(8)]
        public Dictionary<int, ushort> Quests;
        [ProtoMember(9)]
        public Dictionary<int, short> Dialogs;
        [ProtoMember(10)]
        public Dictionary<uint, int> Variables;
        [ProtoMember(11)]
        public Dictionary<int, ushort> Instances;
        [ProtoMember(12)]
        public Dictionary<int, InventorySlot> Items;
        [ProtoMember(13)]
        public Dictionary<uint, Tuple<uint, short, short>> Talents;
        public readonly INetSerializable SerWears;
        public readonly INetSerializable SerSkills;
        public readonly INetSerializable SerTalents;
        public readonly INetSerializable SerInventory;

        public CharData()
        {
            Wears = new Dictionary<int, Item>();
            Skills = new Dictionary<int, int>();
            Variables = new Dictionary<uint, int>();
            Quests = new Dictionary<int, ushort>();
            Dialogs = new Dictionary<int, short>();
            Instances = new Dictionary<int, ushort>();
            Items = new Dictionary<int, InventorySlot>();
            Talents = new Dictionary<uint, Tuple<uint, short, short>>();
            SerWears = new SER_Wears(Wears);
            SerSkills = new SER_Skills(this);
            SerTalents = new SER_Talents(this);
            SerInventory = new SER_Inventory(this);
        }

        public byte[] GetBytes()
        {
            using (var mem = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(mem, this);
                return mem.ToArray();
            }
        }
        public int GetVariable(uint id)
        {
            int ret;
            Variables.TryGetValue(id, out ret);
            return ret;
        }
        public short GetTalentLevel(uint id)
        {
            Tuple<uint, short, short> ret;
            Talents.TryGetValue(id, out ret);
            return ret.Item2;
        }
        public short GetDialogState(ushort id)
        {
            short ret;
            Dialogs.TryGetValue(id, out ret);
            return ret;
        }
    }
}