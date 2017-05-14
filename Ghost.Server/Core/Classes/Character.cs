using PNet;
using System;

namespace Ghost.Server.Core.Classes
{
    public class Character : INetSerializable
    {
        public int Id;
        public int Map;
        public int User;
        public short Level;
        public PonyData Pony;
        public CharData Data;

        public Character(int id, int user, short level, int map, PonyData pony, CharData data)
        {
            Id = id;
            Map = map;
            User = user;
            Pony = pony;
            Data = data;
            Level = level;
        }
        public int AllocSize
        {
            get
            {
                return Pony.AllocSize + 6;
            }
        }
        public void OnSerialize(NetMessage message)
        {
            Pony.OnSerialize(message);
            message.Write(Level);
            message.Write(Id);
        }
        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }
}