using Ghost.Server.Core.Players;
using Ghost.Server.Core.Structs;
using Ghost.Server.Utilities;
using PNet;

namespace Ghost.Server.Core.Classes
{
    public class FriendStatus : INetSerializable
    {
        public int ID;
        public ushort PlayerID;
        public string UserName;
        public OnlineStatus Status;
        public string CharacterName;
        public int AllocSize
        {
            get
            {
                return 15 + (UserName?.Length ?? 0 * 2) + (CharacterName?.Length ?? 0 * 2);
            }
        }
        public FriendStatus Fill(DB_User user, OnlineStatus status)
        {
            ID = user.ID;
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
            PlayerID = player.Player.Id;
            UserName = player.User.Name;
            CharacterName = player.Char.Pony.Name;
            return this;
        }
        public void OnSerialize(NetMessage message)
        {
            message.Write(UserName);
            message.Write(CharacterName);
            message.Write(ID);
            message.Write((byte)Status);
        }
        public void OnDeserialize(NetMessage message)
        {
            Status = (OnlineStatus)message.ReadByte();
            CharacterName = message.ReadString();
            UserName = message.ReadString();
            if (Status > OnlineStatus.Offline)
                PlayerID = message.ReadUInt16();
            ID = message.ReadInt32();
        }
    }
}