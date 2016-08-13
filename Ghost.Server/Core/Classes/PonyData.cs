using PNet;
using ProtoBuf;
using System.IO;
using System.IO.Compression;

namespace Ghost.Server.Core.Classes
{
    [ProtoContract]
    public class PonyData : INetSerializable
    {
        [ProtoMember(1)]
        public short Eye;
        public byte Race;
        [ProtoMember(2)]
        public short Mane;
        [ProtoMember(3)]
        public short Tail;
        [ProtoMember(4)]
        public short Hoof;
        public byte Gender;
        public string Name;
        [ProtoMember(5)]
        public int EyeColor;
        [ProtoMember(6)]
        public int HoofColor;
        [ProtoMember(7)]
        public int BodyColor;
        [ProtoMember(8)]
        public int HairColor0;
        [ProtoMember(9)]
        public int HairColor1;
        [ProtoMember(10)]
        public int HairColor2;
        [ProtoMember(11)]
        public float BodySize;
        [ProtoMember(12)]
        public float HornSize;
        [ProtoMember(13)]
        public int CutieMark0;
        [ProtoMember(14)]
        public int CutieMark1;
        [ProtoMember(15)]
        public int CutieMark2;

        public int AllocSize
        {
            get { return (Name?.Length ?? 0) * 2 + 48; }
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

        public void OnSerialize(NetMessage message)
        {
            message.Write(Name);
            message.Write(Race);
            message.Write(Gender);
            message.Write(CutieMark0);
            message.Write(CutieMark1);
            message.Write(CutieMark2);
            message.Write((byte)(HairColor0 & 0xFF)); message.Write((byte)((HairColor0 >> 8) & 0xFF)); message.Write((byte)((HairColor0 >> 16) & 0xFF));
            message.Write((byte)(HairColor1 & 0xFF)); message.Write((byte)((HairColor1 >> 8) & 0xFF)); message.Write((byte)((HairColor1 >> 16) & 0xFF));
            message.Write((byte)(HairColor2 & 0xFF)); message.Write((byte)((HairColor2 >> 8) & 0xFF)); message.Write((byte)((HairColor2 >> 16) & 0xFF));
            message.Write((byte)(BodyColor & 0xFF)); message.Write((byte)((BodyColor >> 8) & 0xFF)); message.Write((byte)((BodyColor >> 16) & 0xFF));
            message.Write((byte)(EyeColor & 0xFF)); message.Write((byte)((EyeColor >> 8) & 0xFF)); message.Write((byte)((EyeColor >> 16) & 0xFF));
            message.Write((byte)(HoofColor & 0xFF)); message.Write((byte)((HoofColor >> 8) & 0xFF)); message.Write((byte)((HoofColor >> 16) & 0xFF));
            message.Write(Mane);
            message.Write(Tail);
            message.Write(Eye);
            message.Write(Hoof);
            message.Write(BodySize);
            message.WriteRangedSingle(HornSize, 0f, 2f, 16);
        }

        public void OnDeserialize(NetMessage message)
        {
            Name = message.ReadString();
            Race = message.ReadByte();
            Gender = message.ReadByte();
            CutieMark0 = message.ReadInt32();
            CutieMark1 = message.ReadInt32();
            CutieMark2 = message.ReadInt32();
            HairColor0 = message.ReadByte() | (message.ReadByte() << 8) | (message.ReadByte() << 16);
            HairColor1 = message.ReadByte() | (message.ReadByte() << 8) | (message.ReadByte() << 16);
            HairColor2 = message.ReadByte() | (message.ReadByte() << 8) | (message.ReadByte() << 16);
            BodyColor = message.ReadByte() | (message.ReadByte() << 8) | (message.ReadByte() << 16);
            EyeColor = message.ReadByte() | (message.ReadByte() << 8) | (message.ReadByte() << 16);
            HoofColor = message.ReadByte() | (message.ReadByte() << 8) | (message.ReadByte() << 16);
            Mane = message.ReadInt16();
            Tail = message.ReadInt16();
            Eye = message.ReadInt16();
            Hoof = message.ReadInt16();
            BodySize = message.ReadFloat();
            HornSize = message.ReadRangedSingle(0f, 2f, 16);
        }
    }
}