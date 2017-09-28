using Lidgren.Network;
using PNet;
using System;
using System.Collections.Generic;

namespace PNetR.Impl
{
    public class LidgrenDispatchClient : ADispatchClient
    {
        internal NetClient DispatchClient { get; private set; }
        private NetPeerConfiguration _dispatchConfiguration;
        
        private int _lastQueueSize = 16;

        protected internal override void Setup()
        {
            _dispatchConfiguration = new NetPeerConfiguration(Room.Configuration.AppIdentifier + "DPT");

            DispatchClient = new NetClient(_dispatchConfiguration);
        }

        protected internal override void Connect()
        {
            DispatchClient.Start();

            var hailMsg = DispatchClient.CreateMessage(100);
            hailMsg.Write(Room.Configuration.RoomIdentifier);

            //auth data
            hailMsg.Write((int)Room.Configuration.RoomAuthType);
            hailMsg.Write(Room.Configuration.AuthData);
            hailMsg.Write(Room.Configuration.UserDefinedAuthData);

            //connection info
            hailMsg.Write(Room.Configuration.ListenPort);
            hailMsg.Write(Room.Configuration.MaximumPlayers);
            if (Room.Configuration.ListenAddress != null && Room.Configuration.ListenAddress.Trim() != string.Empty)
            {
                hailMsg.Write(Room.Configuration.ListenAddress);
            }
            DispatchClient.Connect(Room.Configuration.DispatcherAddress, Room.Configuration.DispatcherPort, hailMsg);
        }

        protected internal override void ReadQueue()
        {
            if (DispatchClient == null) return;

            var messages = new List<NetIncomingMessage>(_lastQueueSize * 2);
            _lastQueueSize = DispatchClient.ReadMessages(messages);
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < messages.Count; i++)
            {
                var msgType = messages[i].MessageType;
                var msg = NetMessage.GetMessage(messages[i].Data.Length);
                messages[i].Clone(msg);
                msg.Sender = messages[i].SenderConnection;
                DispatchClient.Recycle(messages[i]);

                if (msgType == NetIncomingMessageType.Data)
                {
                    ConsumeData(msg);
                }
                else if (msgType == NetIncomingMessageType.DebugMessage)
                {
                    Debug.Log(msg.ReadString());
                }
                else if (msgType == NetIncomingMessageType.WarningMessage)
                {
                    var str = msg.ReadString();
                    if (!str.StartsWith("Received unhandled library message Acknowledge"))
                        Debug.LogWarning(str);
                }
                else if (msgType == NetIncomingMessageType.StatusChanged)
                {
                    var status = (NetConnectionStatus)msg.ReadByte();
                    var statusReason = msg.ReadString();
                    if (status == NetConnectionStatus.Connected)
                    {
                        var serverConn = DispatchClient.ServerConnection;
                        if (serverConn == null)
                            throw new NullReferenceException("Could not get server connection after connected");
                        var remsg = serverConn.RemoteHailMessage;
                        if (remsg == null)
                            throw new NullReferenceException("Could not get room guid");
                        if (!remsg.ReadBytes(16, out var gid))
                            throw new Exception("Could not read room guid");

                        Connected(new Guid(gid), serverConn);
                    }
                    else if (status == NetConnectionStatus.Disconnected)
                    {
                        Disconnected();
                    }
                    else
                    {
                        Debug.Log($"DConn ServerStatus: {status}, {statusReason}");
                    }
                    UpdateConnectionStatus(status.ToPNet());
                }
                else if (msgType == NetIncomingMessageType.Error)
                {
                    Debug.LogException(new Exception(msg.ReadString()) { Source = "Room.Lidgren.ImplementationQueueRead [Lidgren DispatchClient Error]" }); //this should really never happen...
                }
                else
                {
                    Debug.LogWarning($"Unknown message type {msgType} from server");
                }

                NetMessage.RecycleMessage(msg);
            }
        }

        protected internal override void Disconnect(string reason)
        {
            DispatchClient.Shutdown(reason);
        }

        protected internal override void SendMessage(NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = DispatchClient.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.RoomDelivery();
            DispatchClient.SendMessage(lmsg, method);
        }

        protected internal override NetMessage GetMessage(int size)
        {
            return NetMessage.GetMessage(size);
        }
    }
}
