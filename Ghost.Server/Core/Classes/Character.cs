using Ghost.Server.Mgrs;
using PNet;
using System;

namespace Ghost.Server.Core.Classes
{
    public class Character : INetSerializable
    {
        public int ID;
        public int Map;
        public int User;
        public short Level;
        public PonyData Pony;
        public CharData Data;
        public Character(PonyData pony)
        {
            ID = -1;
            Level = 1;
            Pony = pony;
            CharsMgr.CreateCharacterData(this);
        }
        public Character(int id, int user, short level, int map, PonyData pony, CharData data)
        {
            ID = id;
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
            message.Write(ID);
        }
        public void OnDeserialize(NetMessage message)
        {
            throw new NotSupportedException();
        }
    }
}