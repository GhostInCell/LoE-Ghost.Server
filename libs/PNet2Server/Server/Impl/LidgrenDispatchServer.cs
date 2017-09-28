using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lidgren.Network;
using PNet;

namespace PNetS.Impl
{
    public class LidgrenDispatchServer : ADispatchServer
    {
        internal NetServer RoomServer { get; private set; }
        internal NetServer PlayerServer { get; private set; }

        protected internal override void Initialize()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            //set up room server
            var roomConfig = new NetPeerConfiguration(Server.Configuration.AppIdentifier + "DPT")
            {
                AutoFlushSendQueue = true,
                Port = Server.Configuration.RoomListenPort,
                MaximumConnections = Server.Configuration.MaximumRooms
            };
            roomConfig.SetMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval, true);

            RoomServer = new NetServer(roomConfig);
            RoomServer.RegisterReceivedCallback(RoomCallback);
            RoomServer.Start();

            //set up player server
            var playerConfig = new NetPeerConfiguration(Server.Configuration.AppIdentifier)
            {
                AutoFlushSendQueue = true,
                Port = Server.Configuration.PlayerListenPort,
                MaximumConnections = Server.Configuration.MaximumPlayers
            };
            playerConfig.SetMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval, true);

#if DEBUG
            Debug.Log("Debug build. Simulated latency and packet loss/duplication is enabled.");
            playerConfig.SimulatedLoss = 0.001f;
            playerConfig.SimulatedDuplicatesChance = 0.001f;
            playerConfig.SimulatedMinimumLatency = 0.1f;
            playerConfig.SimulatedRandomLatency = 0.01f;
#endif

            PlayerServer = new NetServer(playerConfig);
            PlayerServer.RegisterReceivedCallback(PlayerCallback);
            PlayerServer.Start();
        }

        #region rooms
        private void RoomCallback(object peer)
        {
            var netPeer = peer as NetServer;
            if (netPeer == null) return;

            var lmsg = netPeer.ReadMessage();

            var msgType = lmsg.MessageType;
            var sender = lmsg.SenderEndPoint;
            var sConn = lmsg.SenderConnection;
            var seq = lmsg.SequenceChannel;
            var msg = NetMessage.GetMessage(lmsg.Data.Length);
            lmsg.Clone(msg);
            msg.Sender = lmsg.SenderConnection;
            netPeer.Recycle(lmsg);

            if (msgType == NetIncomingMessageType.Data)
            {
                var room = GetRoom(sConn);
                if (room != null)
                    ConsumeData(room, msg);
                else
                {
                    Debug.LogError($"Unknown room {sender} sent {msg.LengthBytes} bytes of data");
                    sConn.Disconnect(DtoRMsgs.UnknownRoom);
                }
            }
            else if (msgType == NetIncomingMessageType.DebugMessage)
            {
                Debug.Log(msg.ReadString());
            }
            else if (msgType == NetIncomingMessageType.ConnectionApproval)
            {
                LidgrenApproveRoomConnection(sConn, sender, msg);
            }
            else if (msgType == NetIncomingMessageType.StatusChanged)
            {
                var status = (NetConnectionStatus)msg.ReadByte();
                var statusReason = msg.ReadString();

                if (status == NetConnectionStatus.Connected)
                {
                    var room = sConn.Tag as Room;
                    if (room == null)
                    {
                        Debug.LogError($"A connection joined from {sConn}, but it did not have a room set in the Tag property");
                        sConn.Disconnect(DtoRMsgs.UnknownRoom);
                    }
                    else
                        AddRoom(room);
                }
                else if (status == NetConnectionStatus.Disconnecting || status == NetConnectionStatus.Disconnected)
                {
                    var oldRoom = GetRoom(sConn);
                    if (oldRoom != null)
                    {
                        RemoveRoom(oldRoom);
                        sConn.Tag = null;

                        //todo: move all players in the room to a new room.
                    }
                }

                Debug.Log($"Room status: {status}, {statusReason}");
            }
            else if (msgType == NetIncomingMessageType.WarningMessage)
            {
                Debug.LogWarning(msg.ReadString());
            }
            else if (msgType == NetIncomingMessageType.Error)
            {
                Debug.LogException(new Exception(msg.ReadString()) { Source = "Server.Lidgren.RoomCallback [Errored Lidgren Message]" }); //this should really never happen...
            }

            NetMessage.RecycleMessage(msg);
        }

        private void LidgrenApproveRoomConnection(NetConnection sConn, IPEndPoint sender, NetMessage msg)
        {
            if (!ApproveRoomConnection(sender, msg, out var denyReason, out var room))
            {
                sConn.Deny(denyReason);
                return;
            }

            room.Connection = sConn;

            var gbytes = room.Guid.ToByteArray();
            var gmsg = RoomServer.CreateMessage(gbytes.Length);
            gmsg.Write(gbytes);
            sConn.Approve(gmsg);
            sConn.Tag = room;
        }

        Room GetRoom(NetConnection connection) => connection.Tag as Room;
        #endregion

        #region players
        private void PlayerCallback(object peer)
        {
            var netPeer = peer as NetServer;
            if (netPeer == null) return;

            var lmsg = netPeer.ReadMessage();

            var msgType = lmsg.MessageType;
            var sender = lmsg.SenderEndPoint;
            var sConn = lmsg.SenderConnection;
            var seq = lmsg.SequenceChannel;
            var msg = NetMessage.GetMessage(lmsg.Data.Length);
            lmsg.Clone(msg);
            msg.Sender = lmsg.SenderConnection;
            netPeer.Recycle(lmsg);

            if (msgType == NetIncomingMessageType.Data)
            {
                var player = GetPlayer(sConn);
                if (player != null)
                    ConsumeData(player, msg);
                else
                {
                    Debug.LogError($"Unknown player {sender} sent {msg.LengthBytes} bytes of data");
                    sConn.Disconnect(DtoPMsgs.UnknownPlayer);
                }
            }
            else if (msgType == NetIncomingMessageType.DebugMessage)
            {
                Debug.Log(msg.ReadString());
            }
            else if (msgType == NetIncomingMessageType.ConnectionApproval)
            {
                PlayerAttemptingConnection(sConn, sConn.RemoteEndPoint, player => sConn.Tag = player, msg);
            }
            else if (msgType == NetIncomingMessageType.StatusChanged)
            {
                var status = (NetConnectionStatus)msg.ReadByte();
                var statusReason = msg.ReadString();
                var player = GetPlayer(sConn);
                if (player != null)
                {
                    player.Status = status.ToPNet();
                    if (status == NetConnectionStatus.Disconnecting || status == NetConnectionStatus.Disconnected)
                    {
#if DEBUG
                        if (statusReason == "unequaldisconnect")
                            RemovePlayerNoNotify(player);
                        else
#endif
                        RemovePlayer(player);
                        sConn.Tag = null;
                    }
                    if (status == NetConnectionStatus.Connected)
                    {
                        FinalizePlayerAdd(player);
                    }
                    Debug.Log($"Player status: {player.Status} {statusReason}, {player.Id}");
                }
                else if (status == NetConnectionStatus.RespondedAwaitingApproval)
                {
                    //eh.
                }
                else
                {
                    Debug.Log($"Unknown player {sConn} status: {status}, {statusReason}");
                }
            }
            else if (msgType == NetIncomingMessageType.WarningMessage)
            {
                var str = msg.ReadString();
                if (!str.StartsWith("Received unhandled library message Acknowledge"))
                    Debug.LogWarning(str);
            }
            else if (msgType == NetIncomingMessageType.Error)
            {
                Debug.LogException(new Exception(msg.ReadString()) { Source = "Server.Lidgren.PlayerCallback [Errored Lidgren Message]" }); //this should really never happen...
            }

            NetMessage.RecycleMessage(msg);
        }

        Player GetPlayer(NetConnection connection)
        {
            return connection.Tag as Player;
        }
        #endregion

        protected internal override void SendToAllPlayers(NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.PlayerDelivery(out var seq);
            PlayerServer.SendToAll(lmsg, null, method, seq);
        }
        protected internal override void SendToAllPlayersExcept(Player player, NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.PlayerDelivery(out var seq);
            PlayerServer.SendToAll(lmsg, player.Connection as NetConnection, method, seq);
        }

        protected internal override NetMessage GetMessage(int length)
        {
            return NetMessage.GetMessage(length);
        }

        protected internal override async Task Shutdown(string reason)
        {
            RoomServer.Shutdown(reason);
            PlayerServer.Shutdown(reason);

            while (RoomServer.Status != NetPeerStatus.NotRunning || PlayerServer.Status != NetPeerStatus.NotRunning)
            {
                await Task.Delay(10);
            }
        }

        protected internal override void SendToPlayer(Player player, NetMessage msg, ReliabilityMode mode, bool recycle)
        {
            var lmsg = PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            if (recycle)
                NetMessage.RecycleMessage(msg);

            var method = mode.PlayerDelivery(out var seq);
            PlayerServer.SendMessage(lmsg, player.Connection as NetConnection, method, seq);
        }

        protected internal override void AllowPlayerToConnect(Player player)
        {
            var lmsg = PlayerServer.CreateMessage(2);
            lmsg.Write(player.Id);
            (player.Connection as NetConnection).Approve(lmsg);
        }

        protected internal override void Disconnect(Player player, string reason)
        {
            (player.Connection as NetConnection).Disconnect(reason);
        }

        protected internal override void SendToRoom(Room room, NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = RoomServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.RoomDelivery();
            RoomServer.SendMessage(lmsg, room.Connection as NetConnection, method);
        }

        protected internal override void SendToOtherRooms(Room except, NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = RoomServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.RoomDelivery();
            RoomServer.SendToAll(lmsg, except.Connection as NetConnection, method, 0);
        }

        protected internal override void SendToAllRooms(NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = RoomServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.RoomDelivery();
            RoomServer.SendToAll(lmsg, method);
        }
    }
}
