using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PNet;

namespace PNetR
{
    public partial class Server
    {
        public void Rpc(byte rpcId, params object[] args)
        {
            Rpc(rpcId, RpcMode.ServerOrdered, args);
        }

        public void Rpc(byte rpcId, params INetSerializable[] args)
        {
            int size = 0;
            args.AllocSize(ref size);
            var msg = StartMessage(rpcId, ReliabilityMode.Ordered, size);
            INetSerializableExtensions.WriteParams(ref msg, args);
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        public void Rpc(byte rpcId, RpcMode mode, params object[] args)
        {
            var size = 0;
            foreach (var arg in args)
            {
                if (arg == null)
                    throw new NullReferenceException("Cannot serialize null value");

                size += _room.Serializer.SizeOf(arg);
            }

            var msg = StartMessage(rpcId, mode.ReliabilityMode(), size, mode.BroadcastMode());
            foreach (var arg in args)
            {
                _room.Serializer.Serialize(arg, msg);
            }
            SendMessage(msg, mode.ReliabilityMode());
        }

        NetMessage StartMessage(byte rpcId, ReliabilityMode mode, int size, BroadcastMode broad = BroadcastMode.Server)
        {
            var msg = _room.ServerGetMessage(size + 2);
            msg.Write(RpcUtils.GetHeader(mode, broad, MsgType.Static));
            msg.Write(rpcId);
            return msg;
        }
    }
}
