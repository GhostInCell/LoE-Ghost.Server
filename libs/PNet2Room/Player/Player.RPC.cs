using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PNet;

namespace PNetR
{
    public partial class Player
    {
        public void Rpc(byte rpcId)
        {
            var msg = StartMessage(Room, rpcId, ReliabilityMode.Ordered, 0);
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        public void Rpc(byte rpcId, 
            INetSerializable arg0)
        {
            int size = 0;
            size += arg0.AllocSize;

            var msg = StartMessage(Room, rpcId, ReliabilityMode.Ordered, size);
            arg0.OnSerialize(msg);
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        public void Rpc(byte rpcId, 
            INetSerializable arg0, 
            INetSerializable arg1)
        {
            var size = 0;
            size += arg0.AllocSize;
            size += arg1.AllocSize;

            var msg = StartMessage(Room, rpcId, ReliabilityMode.Ordered, size);
            arg0.OnSerialize(msg);
            arg1.OnSerialize(msg);
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        public void Rpc(byte rpcId,
            INetSerializable arg0,
            INetSerializable arg1,
            INetSerializable arg2)
        {
            int size = 0;
            size += arg0.AllocSize;
            size += arg1.AllocSize;
            size += arg2.AllocSize;

            var msg = StartMessage(Room, rpcId, ReliabilityMode.Ordered, size);
            arg0.OnSerialize(msg);
            arg1.OnSerialize(msg);
            arg2.OnSerialize(msg);
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        public void Rpc(byte rpcId,
            INetSerializable arg0,
            INetSerializable arg1,
            INetSerializable arg2,
            INetSerializable arg3)
        {
            int size = 0;
            size += arg0.AllocSize;
            size += arg1.AllocSize;
            size += arg2.AllocSize;
            size += arg3.AllocSize;

            var msg = StartMessage(Room, rpcId, ReliabilityMode.Ordered, size);
            arg0.OnSerialize(msg);
            arg1.OnSerialize(msg);
            arg2.OnSerialize(msg);
            arg3.OnSerialize(msg);
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        public void Rpc(byte rpcId,
            INetSerializable arg0,
            INetSerializable arg1,
            INetSerializable arg2,
            INetSerializable arg3,
            INetSerializable arg4)
        {
            int size = 0;
            size += arg0.AllocSize;
            size += arg1.AllocSize;
            size += arg2.AllocSize;
            size += arg3.AllocSize;
            size += arg4.AllocSize;

            var msg = StartMessage(Room, rpcId, ReliabilityMode.Ordered, size);
            arg0.OnSerialize(msg);
            arg1.OnSerialize(msg);
            arg2.OnSerialize(msg);
            arg3.OnSerialize(msg);
            arg4.OnSerialize(msg);
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        public void Rpc(byte rpcId, params INetSerializable[] args)
        {
            int size = 0;
            args.AllocSize(ref size);
            var msg = StartMessage(Room, rpcId, ReliabilityMode.Ordered, size);
            INetSerializableExtensions.WriteParams(ref msg, args);
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        public void Rpc(byte rpcId, params object[] args)
        {
            var size = 0;
            foreach (var arg in args)
            {
                if (arg == null)
                    throw new NullReferenceException("Cannot serialize null value");

                size += Room.Serializer.SizeOf(arg);
            }

            var msg = StartMessage(Room, rpcId, ReliabilityMode.Ordered, size);
            foreach (var arg in args)
            {
                Room.Serializer.Serialize(arg, msg);
            }
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        internal static NetMessage StartMessage(Room room, byte rpcId, ReliabilityMode mode, int size)
        {
            var msg = room.RoomGetMessage(size + 2);
            msg.Write(RpcUtils.GetHeader(mode, BroadcastMode.Owner, MsgType.Static));
            msg.Write(rpcId);
            return msg;
        }
    }
}