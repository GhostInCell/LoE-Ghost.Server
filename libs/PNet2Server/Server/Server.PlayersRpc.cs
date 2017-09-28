﻿using System;
using PNet;

namespace PNetS
{
    public partial class Server
    {
        public void AllPlayersRpc<T>(byte rpcId, T arg)
            where T : INetSerializable
        {
            if (arg == null)
                throw new NullReferenceException("Cannot serialize null value");
            var msg = StartMessage(rpcId, ReliabilityMode.Ordered, arg.AllocSize);
            arg.OnSerialize(msg);
            _server.SendToAllPlayers(msg, ReliabilityMode.Ordered);
        }

        public void AllPlayersRpc<T>(Player except, byte rpcId, T arg)
            where T : INetSerializable
        {
            if (arg == null)
                throw new NullReferenceException("Cannot serialize null value");
            var msg = StartMessage(rpcId, ReliabilityMode.Ordered, arg.AllocSize);
            arg.OnSerialize(msg);
            _server.SendToAllPlayersExcept(except, msg, ReliabilityMode.Ordered);
        }

        /// <summary>
        /// send the rpc to all players
        /// </summary>
        /// <param name="rpcId"></param>
        /// <param name="args"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void AllPlayersRpc(byte rpcId, params object[] args)
        {
            var msg = SerializeRpc(rpcId, args);
            _server.SendToAllPlayers(msg, ReliabilityMode.Ordered);
        }

        /// <summary>
        /// send the rpc to all players except except
        /// </summary>
        /// <param name="except"></param>
        /// <param name="rpcId"></param>
        /// <param name="args"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void AllPlayersRpc(Player except, byte rpcId, params object[] args)
        {
            var msg = SerializeRpc(rpcId, args);
            _server.SendToAllPlayersExcept(except, msg, ReliabilityMode.Ordered);
        }

        /// <summary>
        /// Serialize an rpc once, to be used in conjuction with Player.SendMessage. 
        /// YOU SHOULD CALL NetMessage.RecycleMessage WHEN YOU ARE FINISHED WITH IT TO REDUCE GARBAGE COLLECTION
        /// </summary>
        /// <param name="rpcId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public NetMessage SerializeRpc(byte rpcId, params object[] args)
        {
            var size = 0;
            foreach (var arg in args)
            {
                if (arg == null)
                    throw new NullReferenceException("Cannot serialize null value");

                size += Serializer.SizeOf(arg);
            }

            var msg = StartMessage(rpcId, ReliabilityMode.Ordered, size);
            foreach (var arg in args)
            {
                Serializer.Serialize(arg, msg);
            }
            return msg;
        }

        internal void SendToAll(Player except, NetMessage msg, ReliabilityMode reliability)
        {
            _server.SendToAllPlayersExcept(except, msg, reliability);
        }
        internal void SendToAll(NetMessage msg, ReliabilityMode reliability)
        {
            _server.SendToAllPlayers(msg, reliability);
        }
        
        internal NetMessage StartMessage(byte rpcId, ReliabilityMode mode, int size)
        {
            var msg = GetMessage(size + 2);
            msg.Write(RpcUtils.GetHeader(mode, BroadcastMode.Server, MsgType.Static));
            msg.Write(rpcId);
            return msg;
        }

        internal NetMessage StartMessage(byte rpcId, byte subId, ReliabilityMode mode, int size)
        {
            var msg = GetMessage(size + 3);
            msg.Write(RpcUtils.GetHeader(mode, BroadcastMode.Server, MsgType.Static));
            msg.Write(rpcId);
            msg.Write(subId);
            return msg;
        }
        internal void SendPlayerMessage(Player player, NetMessage msg, ReliabilityMode mode, bool recycle)
        {
            _server.SendToPlayer(player, msg, mode, recycle);
        }

        internal void DisconnectPlayer(Player player, string reason)
        {
            _server.Disconnect(player, reason);
        }
    }
}
