using PNet;
using System;

namespace PNetS
{
    public partial class Player
    {
        internal void ConsumeData(NetMessage msg)
        {
            if (Status != ConnectionStatus.Connected)
                return;
            byte header;
            if (!msg.ReadByte(out header))
                return;
            MsgType msgType;
            BroadcastMode broadcast;
            ReliabilityMode reliability;
            SubMsgType sub;
            RpcUtils.ReadHeader(header, out reliability, out broadcast, out msgType, out sub);

            switch (msgType)
            {
                case MsgType.Internal:
                    ProcessInternal(broadcast, reliability, msg);
                    break;
                case MsgType.Static:
                    var info = new PlayerMessageInfo(broadcast){ Reliability = reliability };
                    ProcessStatic(msg, info);
                    break;
                default:
                    Debug.LogWarning($"Unsupported {msg.LengthBytes - 1} byte message of type {msgType} from {this}");
                    break;
            }
        }

        private void ProcessInternal(BroadcastMode broadcast, ReliabilityMode reliability, NetMessage msg)
        {
            try
            {
                var id = msg.ReadByte();
                switch (id)
                {
                    case RandPRpcs.FinishedRoomSwitch:
                        ClientFinishedRoomSwitch();
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void ProcessStatic(NetMessage msg, PlayerMessageInfo info)
        {
            try
            {
                CallRpc(msg.ReadByte(), msg, info);
                //todo: info.continueforwarding

                if (info.ContinueForwarding)
                {
                    if (info.Mode == BroadcastMode.Others)
                    {
                        Server.SendToAll(this, msg, info.Reliability);
                    }
                    else if (info.Mode == BroadcastMode.All)
                    {
                        Server.SendToAll(msg, info.Reliability);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
