using PNet;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PNetS
{
    public abstract class ADispatchServer
    {
        protected internal Server Server { get; internal set; }
        protected internal abstract void Initialize();
        protected internal abstract NetMessage GetMessage(int length);
        protected internal abstract Task Shutdown(string reason);

        protected bool ApproveRoomConnection(IPEndPoint sender, NetMessage msg, out string denyReason, out Room room)
        {
            return Server.ApproveRoomConnection(sender, msg, out denyReason, out room);
        }

        protected void AddRoom(Room room)
        {
            Server.AddRoom(room);
            Debug.Log($"Room connected: {room.Connection} - {room.RoomId} @ {room.Address}");
        }

        protected void RemoveRoom(Room room)
        {
            Server.RemoveRoom(room);
        }

        protected void PlayerAttemptingConnection(object connection, IPEndPoint endpoint, Action<Player> ctor, NetMessage msg)
        {
            Server.PlayerConnecting(connection, endpoint, ctor, msg);
        }

        protected void RemovePlayer(Player player)
        {
            Server.RemovePlayer(player);
        }

#if DEBUG
        protected void RemovePlayerNoNotify(Player player)
        {
            Server.RemovePlayerNoNotify(player);
        }
#endif

        protected void FinalizePlayerAdd(Player player)
        {
            Server.FinalizePlayerAdd(player);
        }

        protected void ConsumeData(Player player, NetMessage msg)
        {
            player.ConsumeData(msg);
        }

        protected void ConsumeData(Room room, NetMessage msg)
        {
            room.ConsumeData(msg);
        }

        protected internal abstract void AllowPlayerToConnect(Player player);
        protected internal abstract void Disconnect(Player player, string reason);

        protected internal abstract void SendToPlayer(Player player, NetMessage msg, ReliabilityMode mode, bool recycle);
        protected internal abstract void SendToAllPlayers(NetMessage msg, ReliabilityMode mode);
        protected internal abstract void SendToAllPlayersExcept(Player player, NetMessage msg, ReliabilityMode mode);
        
        protected internal abstract void SendToRoom(Room room, NetMessage msg, ReliabilityMode mode);
        protected internal abstract void SendToOtherRooms(Room except, NetMessage msg, ReliabilityMode mode);
        protected internal abstract void SendToAllRooms(NetMessage msg, ReliabilityMode mode);
    }
}
