using System.Linq;
#if R_LIDGREN
using PNet;
using System;
using System.Collections.Generic;
using Lidgren.Network;

namespace PNetR
{
    public partial class Room
    {
        internal NetServer PlayerServer { get; private set; }
        private NetPeerConfiguration _serverConfiguration;

        partial void ImplConnectionSetup()
        {
            _serverConfiguration = new NetPeerConfiguration(Configuration.AppIdentifier);
            _serverConfiguration.SetMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval, true);
            _serverConfiguration.Port = Configuration.ListenPort;
            //bit of overhead in connection count
            _serverConfiguration.MaximumConnections = (int)(Configuration.MaximumPlayers * 1.5);

#if DEBUG
            Debug.Log("Debug build. Simulated latency and packet loss/duplication is enabled.");
            _serverConfiguration.SimulatedLoss = 0.001f;
            _serverConfiguration.SimulatedDuplicatesChance = 0.001f;
            _serverConfiguration.SimulatedMinimumLatency = 0.1f;
            _serverConfiguration.SimulatedRandomLatency = 0.01f;
#endif

            PlayerServer = new NetServer(_serverConfiguration);
        }

        partial void ImplRoomStart()
        {
            PlayerServer.Start();
        }

        partial void ImplRoomDisconnect(string reason)
        {
            PlayerServer.Shutdown(reason);
        }

        private int _lastRoomFrameSize = 16;
        partial void ImplRoomRead()
        {
            if (PlayerServer == null) return;

            var messages = new List<NetIncomingMessage>(_lastRoomFrameSize * 2);
            _lastRoomFrameSize = PlayerServer.ReadMessages(messages);

// ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < messages.Count; i++)
            {
                var msg = NetMessage.GetMessage(messages[i].Data.Length);
                var msgType = messages[i].MessageType;
                var method = messages[i].DeliveryMethod;
                var senderConnection = messages[i].SenderConnection;
                messages[i].Clone(msg);
                msg.Sender = senderConnection;
                PlayerServer.Recycle(messages[i]);

                if (msgType == NetIncomingMessageType.Data)
                {
                    var sender = GetPlayer(senderConnection);
                    sender.ConsumeData(msg);
                }
                else if (msgType == NetIncomingMessageType.DebugMessage)
                {
                    Debug.Log(msg.ReadString());
                }
                else if (msgType == NetIncomingMessageType.WarningMessage)
                {
                    Debug.LogWarning(msg.ReadString());
                }
                else if (msgType == NetIncomingMessageType.ConnectionLatencyUpdated)
                {
                    var latency = msg.ReadFloat();
                    //todo: do something with this latency.
                }
                else if (msgType == NetIncomingMessageType.ErrorMessage)
                {
                    Debug.LogError(msg.ReadString());
                }
                else if (msgType == NetIncomingMessageType.ConnectionApproval)
                {
                    var player = ConstructNetData != null ? new Player(this, ConstructNetData()) : new Player(this);
                    player.Connection = senderConnection;
                    senderConnection.Tag = player;
                    if (_players.Count > Configuration.MaximumPlayers)
                    {
                        senderConnection.Deny(DtoPMsgs.NoRoom);
                    }
                    else
                    {
                        Guid token;
                        if (msg.ReadGuid(out token))
                        {
                            VerifyPlayerConnecting(player, token);
                        }
                        else
                        {
                            senderConnection.Deny(DtoPMsgs.BadToken);
                        }
                    }
                }
                else if (msgType == NetIncomingMessageType.StatusChanged)
                {
                    var status = (NetConnectionStatus) msg.ReadByte();
                    var statusReason = msg.ReadString();
                    Debug.Log($"ServerStatus for {status}: {statusReason}, {senderConnection}");

                    if (status == NetConnectionStatus.Connected)
                    {
                        AddPlayer(senderConnection);
                    }
                    else if (status == NetConnectionStatus.Disconnected)
                    {
                        RemovePlayer(senderConnection);
                    }
                }
                else if (msgType == NetIncomingMessageType.Error)
                {
                    Debug.LogException(new Exception(msg.ReadString()){Source = "Room.Lidgren.IplementationQueueRead [Lidgren PlayerServer Error]"}); //this should really never happen...
                }
                else
                {
                    Debug.LogWarning($"Uknown message type {msgType} from player {senderConnection}");
                }

                NetMessage.RecycleMessage(msg);
            }

            VerifyWaitingPlayers();
        }

        private void RemovePlayer(NetConnection senderConnection)
        {
            var player = GetPlayer(senderConnection);
            if (player.Id == 0)
            {
                Debug.LogWarning("Player disconnected with id 0. They probably didn't finish connecting");
            }
            else
            {
                Player oplayer;
                _players.TryGetValue(player.Id, out oplayer);
                if (oplayer != player)
                {
                    Debug.Log($"Finished removing player {player} over contention with id {player.Id}");
                }
                else
                    _players.Remove(player.Id);

                try
                {
                    OnPlayerRemoved(player);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void AddPlayer(NetConnection senderConnection)
        {
            var player = GetPlayer(senderConnection);
            Player oldPlayer;
            _players.TryGetValue(player.Id, out oldPlayer);
            if (oldPlayer != null)
            {
                Debug.LogWarning($"Contention over id {player.Id} : {oldPlayer} is still connected, but should probably not be. Disconnecting");
                oldPlayer.Disconnect("player id contention");
            }
            
            _players[player.Id] = player;
            Debug.Log($"Player {player.Id} joined at {senderConnection}");
            SendViewInstantiates(player);

            var pconnected = ServerGetMessage(4);
            pconnected.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Server, MsgType.Internal));
            pconnected.Write(DandRRpcs.PlayerConnected);
            pconnected.Write(player.Id);
            Server.SendMessage(pconnected, ReliabilityMode.Ordered);
        }

        private Player GetPlayer(NetConnection netConnection)
        {
            return netConnection.Tag as Player;
        }

        partial void ImplSendToPlayers(NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.RoomDelivery();
            PlayerServer.SendToAll(lmsg, method);
        }

        partial void ImplSendToPlayers(Player[] players, NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.RoomDelivery();
            var conns = players.Where(p => p != null).Select(p => p.Connection).ToArray();
            if (conns.Length == 0) return;
            PlayerServer.SendMessage(lmsg, conns, method, 0);
        }

        partial void ImplSendToPlayer(Player player, NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.RoomDelivery();
            PlayerServer.SendMessage(lmsg, player.Connection, method);
        }

        partial void ImplSendExcept(NetMessage msg, Player except, ReliabilityMode mode)
        {
            var lmsg = PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.RoomDelivery();
            PlayerServer.SendToAll(lmsg, except.Connection, method, 0);
        }

        partial void ImplRoomGetMessage(int size, ref NetMessage msg)
        {
            msg = NetMessage.GetMessage(size);
        }
    }
}

#endif