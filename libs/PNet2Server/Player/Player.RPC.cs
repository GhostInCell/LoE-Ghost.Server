using System;
using PNet;
using System.Collections.Generic;
using System.Linq;

namespace PNetS
{
    public partial class Player
    {
        public void PlayerRpc<T>(byte rpcId, T arg)
            where T : INetSerializable
        {
            var msg = Server.StartMessage(rpcId, ReliabilityMode.Ordered, arg.AllocSize);
            arg.OnSerialize(msg);
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        /// <summary>
        /// Send the args as an rpc.
        /// serialization resolution is as follows:
        /// INetSerializable, Server.Serializers, Internal serialization (all built-in structs)
        /// </summary>
        /// <exception cref="NotImplementedException">When serialization does not resolve for any arg</exception>
        /// <param name="rpcId"></param>
        /// <param name="args"></param>
        public void PlayerRpc(byte rpcId, params object[] args)
        {
            var msg = Server.SerializeRpc(rpcId, args);
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        public void PlayerSubRpc<T>(byte rpcId, byte subId, T arg)
            where T : INetSerializable
        {
            var msg = Server.StartMessage(rpcId, subId, ReliabilityMode.Ordered, arg.AllocSize);
            arg.OnSerialize(msg);
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        public void PlayerSubRpc<T>(byte rpcId, byte subId, IEnumerable<T> arg)
            where T : INetSerializable
        {
            var msg = Server.StartMessage(rpcId, subId, ReliabilityMode.Ordered, arg.Sum(x => x.AllocSize) + 4);
            msg.Write(arg.Count());
            foreach (var item in arg)
                item.OnSerialize(msg);
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        /// <summary>
        /// Sends the args as an rpc to the client player.
        /// </summary>
        /// <param name="rpcId"></param>
        /// <param name="args"></param>
        public void PlayerRpc(byte rpcId, params INetSerializable[] args)
        {
            int size = 0;
            args.AllocSize(ref size);
            var msg = Server.StartMessage(rpcId, ReliabilityMode.Ordered, size);
            INetSerializableExtensions.WriteParams(ref msg, args);
            SendMessage(msg, ReliabilityMode.Ordered);
        }

        /// <summary>
        /// send the rpc to all players except this one.
        /// Sends to all if this is Player.ServerPlayer
        /// </summary>
        /// <param name="rpcId"></param>
        /// <param name="args"></param>
        public void OtherPlayersRpc(byte rpcId, params object[] args)
        {
            if (this == ServerPlayer)
            {
                Server.AllPlayersRpc(rpcId, args);
            }
            else
            {
                Server.AllPlayersRpc(this, rpcId, args);
            }
        }
    }
}