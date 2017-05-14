using Ghost.Server.Core.Players;
using Ghost.Server.Core.Structs;
using Ghost.Server.Utilities;
using PNet;
using System;

namespace Ghost.Server.Core.Classes
{
    public class FriendStatus : INetSerializable
    {
        public int ID;
        public string MapID;
        public string MapName;
        public ushort PlayerID;
        public string UserName;
        public short CutieMarkID;
        public CharacterType Race;
        public OnlineStatus Status;
        public DateTime LastOnline;
        public string CharacterName;
        public int AllocSize
        {
            get
            {
                return 25 + (UserName?.Length ?? 0 * 2) + (CharacterName?.Length ?? 0 * 2) + (MapName?.Length ?? 0 * 2) + (MapID?.Length ?? 0 * 2);
            }
        }
        public FriendStatus Fill(DB_User user, OnlineStatus status)
        {
            ID = user.Id;
            PlayerID = 0;
            Status = status;
            UserName = user.Name;
            CharacterName = "none";
            return this;
        }
        public FriendStatus Fill(MasterPlayer player, OnlineStatus status)
        {
            Status = status;
            ID = player.User.ID;
            LastOnline = DateTime.Now;
            PlayerID = player.Player.Id;
            UserName = player.User.Name;
            MapName = player.Player.Room.RoomId;
            CharacterName = player.Char.Pony.Name;
            MapID = player.Player.Room.Guid.ToString();
            Race = (CharacterType)player.Char.Pony.Race;
            CutieMarkID = (short)player.Char.Pony.CutieMark0;
            return this;
        }
        public void OnSerialize(NetMessage message)
        {
            message.Write((byte)Status);
            message.Write(CharacterName);
            message.Write(UserName);
            if (Status > OnlineStatus.Offline)
                message.Write((int)PlayerID);
            message.Write((int)Race);
            message.Write(ID);
            message.Write(CutieMarkID);
            message.Write(MapName);
            message.Write(MapID);
            message.Write(LastOnline);
        }
        public void OnDeserialize(NetMessage message)
        {
            Status = (OnlineStatus)message.ReadByte();
            CharacterName = message.ReadString();
            UserName = message.ReadString();
            if (Status > OnlineStatus.Offline)
                PlayerID = (ushort)message.ReadInt32();
            Race = (CharacterType)message.ReadInt32();
            ID = message.ReadInt32();
            CutieMarkID = message.ReadInt16();
            MapName = message.ReadString();
            MapID = message.ReadString();
            LastOnline = message.ReadDateTime();
        }
    }
}