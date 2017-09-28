using PNet;
using System;

namespace PNetR
{
    public partial class Player
    {
        internal void ConsumeData(NetMessage msg)
        {
            if (!msg.ReadByte(out var header))
                return;
            RpcUtils.ReadHeader(header, out var reliability, out var broadcast, out var msgType, out var sub);

            switch (msgType)
            {
                case MsgType.Internal:
                    ProcessInternal(broadcast, reliability, msg);
                    break;
                case MsgType.Static:
                    ProcessStatic(broadcast, reliability, msg);
                    break;
                case MsgType.Netview:
                    var info = new NetMessageInfo(broadcast, this){Reliability = reliability};
                    Room.NetworkManager.CallRpc(msg, info, sub);
                    break;
                case MsgType.Stream:
                    Room.NetworkManager.Stream(msg, this);
                    break;
                default:
                    Debug.LogWarning($"Unsupported {msg.LengthBytes - 1} byte message of type {msgType} from {this}");
                    break;
            }
        }

        private void ProcessInternal(BroadcastMode broadcast, ReliabilityMode reliability, NetMessage msg)
        {
            var rpc = msg.ReadByte();
            switch (rpc)
            {
                case RandPRpcs.Instantiate:
                    Room.NetworkManager.FinishedInstantiate(this, msg);
                    break;
                case RandPRpcs.SceneObjectRpc:
                    Room.SceneViewManager.CallRpc(msg, new NetMessageInfo(broadcast, this));
                    break;
            }
        }

        private void ProcessStatic(BroadcastMode broadcast, ReliabilityMode reliability, NetMessage msg)
        {
            var info = new NetMessageInfo(broadcast, this) { Reliability = reliability };

            //route back to room...
            try
            {
                Room.CallRpc(msg, info);
            }
            catch (Exception e)
            {
                info.ContinueForwarding = false;
                Debug.LogException(e);
            }

            //todo: filter if rpc mode is all/others/owner, and then send to appropriate people.
            if (info.ContinueForwarding)
            {
                if (info.Mode == BroadcastMode.Others)
                {
                    Room.SendExcept(msg, info.Sender, info.Reliability);
                }
                else if (info.Mode == BroadcastMode.All)
                {
                    Room.SendToPlayers(msg, info.Reliability);
                }
            }
        }
    }
}
