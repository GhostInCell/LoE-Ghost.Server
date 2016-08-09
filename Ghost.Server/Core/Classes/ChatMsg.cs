using Ghost.Server.Utilities;
using PNet;
using System;

namespace Ghost.Server.Core.Classes
{
    public class ChatMsg : INetSerializable
    {
        public string Text;
        public string Name;
        public int PlayerID;
        public ChatIcon Icon;
        public ChatType Type;
        public DateTime Time;
        public UserData User;
        public int AllocSize
        {
            get
            {
                return 20 + User.Name.Length * 2 + Text.Length * 2;
            }
        }
        public ChatMsg(int player, UserData user)
        {
            User = user;
            PlayerID = player;
        }
        public void OnSerialize(NetMessage message)
        {
            message.Write((byte)Type);
            message.Write(Name ?? User.Name);
            message.Write(Text);
            message.Write(Time);
            message.Write(User.Char);
            message.Write((byte)Icon);
            message.Write(PlayerID);
        }
        public void OnDeserialize(NetMessage message)
        {
            Type = (ChatType)message.ReadByte();
            Text = message.ReadString();
            Icon = (ChatIcon)message.ReadByte();
            Time = DateTime.Now;
        }
    }
}