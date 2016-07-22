using PNet;
#if S_LIDGREN
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PNetR
{
    partial class Room
    {
        internal NetClient DispatchClient { get; private set; }
        private NetPeerConfiguration _dispatchConfiguration;

        partial void ImplDispatchSetup()
        {
            _dispatchConfiguration = new NetPeerConfiguration(Configuration.AppIdentifier + "DPT");
            DispatchClient = new NetClient(_dispatchConfiguration);
        }

        partial void ImplDispatchConnect()
        {
            DispatchClient.Start();

            var hailMsg = DispatchClient.CreateMessage(100);
            hailMsg.Write(Configuration.RoomIdentifier);

            //auth data
            hailMsg.Write((int)Configuration.RoomAuthType);
            hailMsg.Write(Configuration.AuthData);
            hailMsg.Write(Configuration.UserDefinedAuthData);

            //connection info
            hailMsg.Write(_serverConfiguration.Port);
            hailMsg.Write(Configuration.MaximumPlayers);
            if (Configuration.ListenAddress != null && Configuration.ListenAddress.Trim() != string.Empty)
            {
                hailMsg.Write(Configuration.ListenAddress);
            }
            DispatchClient.Connect(Configuration.DispatcherAddress, Configuration.DispatcherPort, hailMsg);
        }

        private int _lastDispatchSize = 16;
        partial void ImplDispatchRead()
        {
            if (DispatchClient == null) return;

            var messages = m_messages;
            _lastDispatchSize = DispatchClient.ReadMessages(messages);
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
                    if (Server != null)
                        Server.ConsumeData(msg);
                    else
                    {
                        Debug.LogWarning("Received server data when not connected");
                    }
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
                        byte[] gid;
                        if (!remsg.ReadBytes(16, out gid))
                            throw new Exception("Could not read room guid");
                        RoomId = new Guid(gid);
                        Debug.Log($"Connected to dispatcher. Id is {RoomId}");
                        Server = new Server(this) { Connection = serverConn };
                    }
                    else if (status == NetConnectionStatus.Disconnected)
                    {
                        Server = null;
                        RoomId = Guid.Empty;
                        Debug.Log("Disconnected from dispatcher");
                    }
                    else
                    {
                        Debug.Log($"DConn ServerStatus: {status}, {statusReason}");
                    }
                    ServerStatus = status.ToPNet();
                    ServerStatusChanged?.Invoke();
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
            messages.Clear();
        }

        partial void ImplDispatchDisconnect(string reason)
        {
            DispatchClient.Shutdown(reason);
        }

        partial void ImplServerGetMessage(int size, ref NetMessage msg)
        {
            msg = NetMessage.GetMessage(size);
        }
    }
}
#endif