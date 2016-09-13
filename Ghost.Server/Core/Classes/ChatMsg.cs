using Ghost.Server.Utilities;
using PNet;
using System;

namespace Ghost.Server.Core.Classes
{
    public struct ChatMsg : INetSerializable
    {
        public static readonly ChatMsg System = new ChatMsg()
        {
            Name = string.Empty,
            Type = ChatType.System,
            Icon = ChatIcon.System,
            CharID = -1,
            PlayerID = -1
        };

        public int CharID;
        public string Text;
        public string Name;
        public int PlayerID;
        public ChatIcon Icon;
        public ChatType Type;
        public DateTime Time;
        public int AllocSize
        {
            get
            {
                return 20 + Name.Length * 2 + Text.Length * 2;
            }
        }

        public void OnSerialize(NetMessage message)
        {
            message.Write((byte)Type);
            message.Write(Name);
            message.Write(Text);
            message.Write(Time);
            message.Write(CharID);
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