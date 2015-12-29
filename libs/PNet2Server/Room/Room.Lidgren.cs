#if LIDGREN
using PNet;
using Lidgren.Network;

namespace PNetS
{
    public partial class Room
    {
        internal NetConnection Connection { get; set; }

        partial void ImplementationSendMessage(NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = _server.RoomServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.RoomDelivery();
            _server.RoomServer.SendMessage(lmsg, Connection, method);
        }

        partial void ImplSendMessageToOthers(NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = _server.RoomServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.RoomDelivery();
            _server.RoomServer.SendToAll(lmsg, Connection, method, 0);
        }

        partial void ImplSendToAll(NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = _server.RoomServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.RoomDelivery();
            _server.RoomServer.SendToAll(lmsg, method);
        }
    }
}
#endif