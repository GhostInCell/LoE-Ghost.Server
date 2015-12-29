#if R_LIDGREN
using Lidgren.Network;
using PNet;

namespace PNetR
{
    public partial class Player
    {
        internal NetConnection Connection;

        partial void ImplementationSendMessage(NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = Room.PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            int seq;
            var method = mode.PlayerDelivery(out seq);
            Room.PlayerServer.SendMessage(lmsg, Connection, method, seq);
        }

        partial void ImplementationAllowConnect()
        {
            var msg = Room.PlayerServer.CreateMessage(16);
            msg.Write(Room.RoomId.ToByteArray());
            Connection.Approve(msg);
        }

        partial void ImplementationDisconnect(string reason)
        {
            Connection.Disconnect(reason);
        }
    }
}
#endif