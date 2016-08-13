using Ghost.Server.Utilities;
using PNet;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Numerics;

namespace Ghost.Server.Core.Classes
{
    [ProtoContract]
    public struct TalentData
    {
        [ProtoMember(1)]
        public uint Exp;
        [ProtoMember(2)]
        public short Level;
        [ProtoMember(3)]
        public short Points;
    }
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
        public Dictionary<int, int> Skills;
        [ProtoMember(7)]
        public Dictionary<int, ushort> Quests;
        [ProtoMember(8)]
        public Dictionary<int, short> Dialogs;
        [ProtoMember(9)]
        public Dictionary<uint, int> Variables;
        [ProtoMember(10)]
        public Dictionary<int, ushort> Instances;
        [ProtoMember(11)]
        public Dictionary<int, InventorySlot> Items;
        [ProtoMember(12)]
        public Dictionary<int, InventoryItem> Wears;
        [ProtoMember(13)]
        public Dictionary<TalentMarkId, TalentData> Talents;
        public readonly INetSerializable SerWears;
        public readonly INetSerializable SerSkills;
        public readonly INetSerializable SerTalents;
        public readonly INetSerializable SerInventory;

        public CharData()
        {
            Skills = new Dictionary<int, int>();
            Variables = new Dictionary<uint, int>();
            Quests = new Dictionary<int, ushort>();
            Dialogs = new Dictionary<int, short>();
            Instances = new Dictionary<int, ushort>();
            Wears = new Dictionary<int, InventoryItem>();
            Items = new Dictionary<int, InventorySlot>();
            Talents = new Dictionary<TalentMarkId, TalentData>();
            SerWears = new SER_Wears(Wears);
            SerSkills = new SER_Skills(this);
            SerTalents = new SER_Talents(this);
            SerInventory = new SER_Inventory(this);
        }

        public byte[] GetBytes()
        {
            using (var mem = new MemoryStream())
            {
                using (var zip = new DeflateStream(mem, CompressionLevel.Optimal, true))
                    ProtoBuf.Serializer.Serialize(zip, this);
                return mem.ToArray();
            }
        }
        public int GetVariable(uint id)
        {
            int ret;
            Variables.TryGetValue(id, out ret);
            return ret;
        }
        public short GetTalentLevel(TalentMarkId id)
        {
            TalentData ret;
            Talents.TryGetValue(id, out ret);
            return ret.Level;
        }
        public short GetDialogState(ushort id)
        {
            short ret;
            Dialogs.TryGetValue(id, out ret);
            return ret;
        }
    }
}