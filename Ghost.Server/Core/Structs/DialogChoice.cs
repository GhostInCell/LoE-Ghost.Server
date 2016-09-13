using System;
using Ghost.Server.Core.Players;
using Ghost.Server.Mgrs;
using Ghost.Server.Utilities;
using PNet;

namespace Ghost.Server.Core.Structs
{
    public struct DialogChoice : INetSerializable
    {
        public short State;
        public string Message;

        public int AllocSize
        {
            get
            {
               return Message.Length * 2;
            }
        }

        public DialogChoice(short state, int message, MapPlayer player)
        {
            State = state;
            Message = DataMgr.SelectMessage(message).Item2.GetMessage(player);
        }

        public void OnSerialize(NetMessage message)
        {
            message.Write(Message);
        }

        public void OnDeserialize(NetMessage message)
        {
            Message = message.ReadString();
        }
    }
}