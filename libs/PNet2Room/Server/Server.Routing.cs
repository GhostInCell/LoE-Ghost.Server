using System;
using PNet;

namespace PNetR
{
    public partial class Server
    {
        internal void ConsumeData(NetMessage msg)
        {
            if (!msg.ReadByte(out var header))
                return;
            RpcUtils.ReadHeader(header, out var reliability, out var broadcast, out var msgType, out var sub);

            switch (msgType)
            {
                case MsgType.Internal:
                    ProcessInternal(msg);
                    break;
                case MsgType.Static:
                    ProcessStatic(broadcast, reliability, msg);
                    break;
                default:
                    Debug.LogWarning($"Unsupported {msg.LengthBytes - 1} byte message of type {msgType} from {this}");
                    break;
            }
        }

        private void ProcessStatic(BroadcastMode broadcast, ReliabilityMode reliability, NetMessage msg)
        {
            try
            {
                if (!msg.ReadByte(out var rpc))
                    return;
                CallRpc(rpc, msg);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void ProcessInternal(NetMessage msg)
        {
            var rpc = msg.ReadByte();
            switch (rpc)
            {
                case DandRRpcs.ExpectPlayer:
                    _room.PlayerWillConnect(msg);
                    break;
                case DandRRpcs.ExpectLeavingPlayer:
                    //todo
                    break;
                case DandRRpcs.RoomSwitch:
                    break;
                case DandRRpcs.DisconnectPlayer:
                    var pid = msg.ReadUInt16();
                    var player = _room.GetPlayer(pid);
                    if (player != null)
                    {
                        //because clients ignore roomswitch disconnects
                        player.Disconnect(PtoDMsgs.RoomSwitch);
                    }
                    break;
                case DandRRpcs.SyncNetUser:
                    pid = msg.ReadUInt16();
                    player = _room.GetPlayer(pid);
                    if (player != null)
                    {
                        if (player.NetUserData != null)
                        {
                            player.NetUserData.OnDeserialize(msg);
                            player.OnNetUserDataChanged();
                        }
                        else
                            Debug.LogWarning($"Received user data from dispatcher for {pid}, but they don't have a netuserdata set up to deserialize it");
                    }
                    else
                    {
                        Debug.LogWarning($"Received user data from dispatcher for {pid}, but they could not be found");
                    }
                    break;
                case DandRRpcs.RoomAdd:
                    while (msg.RemainingBits > 128)
                    {
                        if (!msg.ReadString(out var roomId))
                            break;
                        if (!msg.ReadGuid(out var guid))
                            break;
                        _rooms[guid] = roomId;
                        _roomNames.TryGetValue(roomId, out var val);
                        //relying on the fact that default for tryget will make val 0
                        _roomNames[roomId] = ++val;
                    }
                    break;
                case DandRRpcs.RoomRemove:
                    while (msg.RemainingBits >= 128)
                    {
                        if (!msg.ReadGuid(out var guid))
                            break;
                        if (_rooms.TryGetValue(guid, out var roomId))
                        {
                            var val = _roomNames[roomId];
                            if (val == 1)
                                _roomNames.Remove(roomId);
                            else
                                _roomNames[roomId] = --val;
                        }
                    }
                    break;
                case DandRRpcs.DisconnectMessage:
                    string reason;
                    msg.ReadString(out reason);
                    _room.Shutdown(reason);
                    break;
            }
        }
    }
}
