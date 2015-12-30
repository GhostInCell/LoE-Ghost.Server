using Ghost.Server.Utilities;
using PNet;
using ProtoBuf;
using ProtoBuf.Meta;
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
        public byte InvSlots;
        [ProtoMember(4)]
        public Vector3 Position;
        [ProtoMember(5)]
        public Vector3 Rotation;
        [ProtoMember(6)]
        public Dictionary<byte, int> Wears;
        [ProtoMember(7)]
        public Dictionary<int, int> Skills;
        [ProtoMember(8)]
        public Dictionary<uint, int> Variables;
        [ProtoMember(9)]
        public Dictionary<ushort, ushort> Quests;
        [ProtoMember(10)]
        public Dictionary<ushort, short> Dialogs;
        [ProtoMember(11)]
        public Dictionary<ushort, ushort> Instances;
        [ProtoMember(12)]
        public Dictionary<byte, Tuple<int, int>> Items;
        [ProtoMember(13)]
        public Dictionary<int, Tuple<uint, short>> Talents;
        public readonly INetSerializable SerWears;
        public readonly INetSerializable SerSkills;
        public readonly INetSerializable SerTalents;
        public readonly INetSerializable SerInventory;
        public CharData()
        {
            Wears = new Dictionary<byte, int>();
            Skills = new Dictionary<int, int>();
            Variables = new Dictionary<uint, int>();
            Quests = new Dictionary<ushort, ushort>();
            Dialogs = new Dictionary<ushort, short>();
            Instances = new Dictionary<ushort, ushort>();
            Items = new Dictionary<byte, Tuple<int, int>>();
            Talents = new Dictionary<int, Tuple<uint, short>>();
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
        public short GetTalentLevel(int id)
        {
            Tuple<uint, short> ret;
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