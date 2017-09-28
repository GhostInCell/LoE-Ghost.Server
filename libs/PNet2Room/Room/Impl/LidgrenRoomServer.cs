using Lidgren.Network;
using PNet;
using System;
using System.Collections.Generic;

namespace PNetR.Impl
{
    public class LidgrenRoomServer : ARoomServer
    {
        private NetServer _playerServer;
        private NetPeerConfiguration _serverConfiguration;

        private int _lastRoomFrameSize = 16;

        protected internal override void Setup()
        {
            _serverConfiguration = new NetPeerConfiguration(Room.Configuration.AppIdentifier);
            _serverConfiguration.SetMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval, true);
            _serverConfiguration.Port = Room.Configuration.ListenPort;
            //bit of overhead in connection count
            _serverConfiguration.MaximumConnections = (int)(Room.Configuration.MaximumPlayers * 1.5);

#if DEBUG
            Debug.Log("Debug build. Simulated latency and packet loss/duplication is enabled.");
            _serverConfiguration.SimulatedLoss = 0.001f;
            _serverConfiguration.SimulatedDuplicatesChance = 0.001f;
            _serverConfiguration.SimulatedMinimumLatency = 0.1f;
            _serverConfiguration.SimulatedRandomLatency = 0.01f;
#endif

            _playerServer = new NetServer(_serverConfiguration);
        }

        protected internal override void Start()
        {
            if (_playerServer.Status != NetPeerStatus.NotRunning) return;
            _playerServer.Start();
        }

        protected internal override void Shutdown(string reason)
        {
            _playerServer.Shutdown(reason);
        }

        protected internal override NetMessage GetMessage(int size)
        {
            return NetMessage.GetMessage(size);
        }

        protected internal override void ReadQueue()
        {
            if (_playerServer == null) return;

            var messages = new List<NetIncomingMessage>(_lastRoomFrameSize * 2);
            _lastRoomFrameSize = _playerServer.ReadMessages(messages);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < messages.Count; i++)
            {
                var msg = NetMessage.GetMessage(messages[i].Data.Length);
                var msgType = messages[i].MessageType;
                var method = messages[i].DeliveryMethod;
                var senderConnection = messages[i].SenderConnection;
                messages[i].Clone(msg);
                msg.Sender = senderConnection;
                _playerServer.Recycle(messages[i]);

                if (msgType == NetIncomingMessageType.Data)
                {
                    ConsumeData(GetPlayer(senderConnection), msg);
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
                    var player = ConstructNewPlayer(senderConnection);
                    player.EndPoint = senderConnection.RemoteEndPoint;
                    senderConnection.Tag = player;
                    if (Room.PlayerCount > Room.Configuration.MaximumPlayers)
                    {
                        senderConnection.Deny(DtoPMsgs.NoRoom);
                    }
                    else
                    {
                        if (msg.ReadGuid(out var token))
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
                    var status = (NetConnectionStatus)msg.ReadByte();
                    var statusReason = msg.ReadString();
                    Debug.Log($"ServerStatus for {senderConnection}: {status}, {statusReason}");

                    if (status == NetConnectionStatus.Connected)
                    {
                        AddingPlayer(GetPlayer(senderConnection));
                    }
                    else if (status == NetConnectionStatus.Disconnected)
                    {
                        RemovePlayer(GetPlayer(senderConnection));
                    }
                }
                else if (msgType == NetIncomingMessageType.Error)
                {
                    Debug.LogException(new Exception(msg.ReadString()) { Source = "Room.Lidgren.IplementationQueueRead [Lidgren PlayerServer Error]" }); //this should really never happen...
                }
                else
                {
                    Debug.LogWarning($"Uknown message type {msgType} from player {senderConnection}");
                }

                NetMessage.RecycleMessage(msg);
            }

            VerifyWaitingPlayers();
        }

        private Player GetPlayer(NetConnection netConnection)
        {
            return netConnection.Tag as Player;
        }

        protected internal override void SendToPlayers(NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = _playerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.RoomDelivery();
            _playerServer.SendToAll(lmsg, method);
        }

        protected internal override void SendToPlayers(List<Player> players, NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = _playerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.PlayerDelivery(out var seq);
            var conns = new List<NetConnection>(players.Count);
// ReSharper disable once ForCanBeConvertedToForeach speed is necessary
            for(int i = 0; i < players.Count; i++)
            {
                if (players[i] == null) continue;
                conns.Add(players[i].Connection as NetConnection);
            }
            if (conns.Count == 0) return;
            _playerServer.SendMessage(lmsg, conns, method, seq);
        }

        protected internal override void SendToPlayer(Player player, NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = _playerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.PlayerDelivery(out var seq);
            _playerServer.SendMessage(lmsg, player.Connection as NetConnection, method, seq);
        }

        protected internal override void SendExcept(NetMessage msg, Player except, ReliabilityMode mode)
        {
            var lmsg = _playerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.PlayerDelivery(out var seq);
            _playerServer.SendToAll(lmsg, except.Connection as NetConnection, method, seq);
        }

        protected internal override void SendSceneView(NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = _playerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.PlayerDelivery(out var seq);
            if (seq == 2) seq = 3; //don't use player channel...

            _playerServer.SendToAll(lmsg, null, method, seq);
        }

        protected internal override void AllowConnect(Player player)
        {
            var msg = _playerServer.CreateMessage(16);
            msg.Write(Room.RoomId.ToByteArray());
            (player.Connection as NetConnection).Approve(msg);
        }

        protected internal override void Disconnect(Player player, string reason)
        {
            (player.Connection as NetConnection).Disconnect(reason);
        }

        protected internal override void SendToConnections(List<object> connections, NetMessage msg, ReliabilityMode reliable)
        {
            var lmsg = _playerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = reliable.PlayerDelivery(out var seq);
            var conns = new List<NetConnection>(connections.Count);
// ReSharper disable once ForCanBeConvertedToForeach speed is necessary
            for (var i = 0; i < connections.Count; i++)
            {
                conns.Add(connections[i] as NetConnection);
            }
            _playerServer.SendMessage(lmsg, conns, method, seq);
        }
    }
}
