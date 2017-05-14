using System.Net;
using System.Threading;
using PNet;
#if LIDGREN
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PNetS
{
    public partial class Server
    {
        internal NetServer RoomServer { get; private set; }
        internal NetServer PlayerServer { get; private set; }

        partial void InternalInitialize()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            //set up room server
            var roomConfig = new NetPeerConfiguration(Configuration.AppIdentifier + "DPT");
            roomConfig.AutoFlushSendQueue = true;
            roomConfig.Port = Configuration.RoomListenPort;
            roomConfig.MaximumConnections = Configuration.MaximumRooms;
            roomConfig.SetMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval, true);
            
            RoomServer = new NetServer(roomConfig);
            RoomServer.RegisterReceivedCallback(RoomCallback);
            RoomServer.Start();
            
            //set up player server
            var playerConfig = new NetPeerConfiguration(Configuration.AppIdentifier);
            playerConfig.AutoFlushSendQueue = true;
            playerConfig.Port = Configuration.PlayerListenPort;
            playerConfig.MaximumConnections = Configuration.MaximumPlayers;
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

        partial void ImplementationShutdown(string reason)
        {
            RoomServer.Shutdown(reason);
            PlayerServer.Shutdown(reason);
        }

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
                    room.ConsumeData(msg);
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
                    if (sConn.Tag is Room room)
                    {
                        AddRoom(room);
                        UpdateRoomsOfNewRoom(room);

                        Debug.Log($"Room connected: {room.RoomId} - {sender} @ {room.Address}");
                    }
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
                Debug.LogException(new Exception(msg.ReadString()){Source = "Server.Lidgren.RoomCallback [Errored Lidgren Message]"}); //this should really never happen...
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

        Room GetRoom(NetConnection connection)
        {
            return connection.Tag as Room;
        }

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
                    player.ConsumeData(msg);
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
                var player = new Player(this) {Connection = sConn};
                sConn.Tag = player;
                player.Status = ConnectionStatus.Connecting;
                if (ConstructNetData != null)
                    player.NetUserData = ConstructNetData();

                if (VerifyPlayer != null)
                {
                    VerifyPlayer(player, msg);
                }
                else
                {
                    player.AllowConnect();
                }
            }
            else if (msgType == NetIncomingMessageType.StatusChanged)
            {
                var status = (NetConnectionStatus) msg.ReadByte();
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
                    Debug.Log($"Player status: {player.Status}, {statusReason}");
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

        partial void ImplSendToAllPlayers(NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            int seq;
            var method = mode.PlayerDelivery(out seq);
            PlayerServer.SendToAll(lmsg, null, method, seq);
        }
        partial void ImplSendToAllPlayersExcept(Player player, NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            int seq;
            var method = mode.PlayerDelivery(out seq);
            PlayerServer.SendToAll(lmsg, player.Connection, method, seq);
        }

        partial void ImplGetMessage(int length, ref NetMessage message)
        {
            message = NetMessage.GetMessage(length);
        }
    }

}
#endif