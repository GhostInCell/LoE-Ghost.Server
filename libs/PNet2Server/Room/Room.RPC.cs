using PNet;
using System;

namespace PNetS
{
    public partial class Room
    {
        public void Rpc(byte rpcId, params object[] args)
        {
            var size = 0;
            var idx = 0;
            foreach (var arg in args)
            {
                if (arg == null)
                    throw new ArgumentNullException($"args[{idx}]", "Cannot serialize null value");
                
                size += _server.Serializer.SizeOf(arg);
                idx++;
            }

            var msg = StartMessage(rpcId, ReliabilityMode.Ordered, size);
            foreach (var arg in args)
            {
                _server.Serializer.Serialize(arg, msg);
            }
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        public void Rpc(byte rpcId, params INetSerializable[] args)
        {
            int size = 0;
            args.AllocSize(ref size);
            var msg = StartMessage(rpcId, ReliabilityMode.Ordered, size);
            INetSerializableExtensions.WriteParams(ref msg, args);
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        NetMessage StartMessage(byte rpcId, ReliabilityMode mode, int size)
        {
            var msg = _server.GetMessage(size + 2);
            msg.Write(RpcUtils.GetHeader(mode, BroadcastMode.Server, MsgType.Static));
            msg.Write(rpcId);
            return msg;
        }
    }
}
