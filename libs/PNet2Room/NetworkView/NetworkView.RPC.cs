using PNet;
using System;
using System.Collections.Generic;

namespace PNetR
{
    public partial class NetworkView
    {
        private readonly Dictionary<ushort, Dictionary<int, Queue<AContinuation>>> _continuations =
            new Dictionary<ushort, Dictionary<int, Queue<AContinuation>>>();

        public void Rpc<TComp>(byte rpcId, RpcMode mode)
        {
            if (!GetCompId<TComp>(out var cid)) return;
            Rpc(cid, rpcId, mode);
        }

        public void Rpc<TComp>(byte rpcId, RpcMode mode, params object[] args)
            where TComp : class
        {
            if (!GetCompId<TComp>(out var cid)) return;
            Rpc(cid, rpcId, mode, args);
        }

        public void Rpc<TComp>(byte rpcId, Player player, params object[] args)
            where TComp : class
        {
            if (!GetCompId<TComp>(out var cid)) return;

            Rpc(cid, rpcId, player, args);
        }

        bool GetCompId<TComp>(out byte id)
        {
            if (!typeof(TComp).GetNetId(out id))
            {
                Debug.LogError($"Could not get NetworkComponentAttribute from type {typeof(TComp)}");
                return false;
            }
            return true;
        }

        public void Rpc(byte compId, byte rpcId, RpcMode mode)
        {
            var msg = StartMessage(compId, rpcId, mode, 0);
            SendMessage(msg, mode);
        }

        public void Rpc(byte compId, byte rpcId, Player player)
        {
            var msg = StartMessage(compId, rpcId, ReliabilityMode.Ordered, 0);
            SendMessage(msg, player, ReliabilityMode.Ordered);
        }

        public void Rpc<T>(byte compId, byte rpcId, RpcMode mode, T arg)
            where T : INetSerializable
        {
            var msg = StartMessage(compId, rpcId, mode, arg.AllocSize);
            arg.OnSerialize(msg);
            SendMessage(msg, mode);
        }

        public void Rpc<T>(byte compId, byte rpcId, Player player, T arg)
            where T : INetSerializable
        {
            var msg = StartMessage(compId, rpcId, ReliabilityMode.Ordered, arg.AllocSize);
            arg.OnSerialize(msg);
            SendMessage(msg, player, ReliabilityMode.Ordered);
        }

        public void Rpc(byte compId, byte rpcId, RpcMode mode, params object[] args)
        {
            var size = 0;
            foreach (var arg in args)
            {
                if (arg == null)
                    throw new NullReferenceException("Cannot serialize null value");

                size += Manager.Room.Serializer.SizeOf(arg);
            }

            var msg = StartMessage(compId, rpcId, mode, size);
            foreach (var arg in args)
            {
                Manager.Room.Serializer.Serialize(arg, msg);
            }
            SendMessage(msg, mode);
        }

        public void Rpc(byte compId, byte rpcId, Player player, params object[] args)
        {
            var size = 0;
            foreach (var arg in args)
            {
                if (arg == null)
                    throw new NullReferenceException("Cannot serialize null value");

                size += Manager.Room.Serializer.SizeOf(arg);
            }

            var msg = StartMessage(compId, rpcId, ReliabilityMode.Ordered, size);
            foreach (var arg in args)
            {
                Manager.Room.Serializer.Serialize(arg, msg);
            }
            SendMessage(msg, player, ReliabilityMode.Ordered);
        }

        public Continuation RpcContinueWith(byte compId, byte rpcId, Player player, params object[] args)
        {
            Rpc(compId, rpcId, player, args);

            //don't actually enqueue it. it'll get discarded at some point.
            if (!player.IsValid)
                return new Continuation();

            var cont = new Continuation(compId, rpcId);
            Enqueue(cont, player.Id);
            return cont;
        }

        public Continuation<T> RpcContinueWith<T>(byte compId, byte rpcId, Player player, params object[] args)
        {
            Rpc(compId, rpcId, player, args);

            //don't actually enqueue it. it'll get discarded at some point.
            if (!player.IsValid)
                return new Continuation<T>();
            
            var ser = Room.Serializer;
            var cont = new Continuation<T>(compId, rpcId, message =>
            {
                if (ser.CanDeserialize(typeof(T)))
                    return (T)ser.Deserialize(typeof(T), message);
                return default(T);
            });
            Enqueue(cont, player.Id);
            return cont;
        }

        void Enqueue(AContinuation continuation, ushort playerId)
        {
            //todo: timeouts
            if (!_continuations.TryGetValue(playerId, out var playerQueues))
            {
                playerQueues = new Dictionary<int, Queue<AContinuation>>();
                _continuations[playerId] = playerQueues;
            }

            var id = (continuation.ComponentId << 8) | continuation.RpcId;

            if (!playerQueues.TryGetValue(id, out var queue))
            {
                queue = new Queue<AContinuation>();
                playerQueues[id] = queue;
            }
            
            queue.Enqueue(continuation);
        }

        AContinuation Dequeue(ushort playerId, byte compId, byte rpcId)
        {
            if (!_continuations.TryGetValue(playerId, out var playerQueues))
                return null;

            var id = (compId << 8) | rpcId;

            if (!playerQueues.TryGetValue(id, out var queue))
                return null;

            return queue.Count > 0 ? queue.Dequeue() : null;
        }


        NetMessage StartMessage(byte cid, byte rpcId, RpcMode mode, int size)
        {
            var msg = Room.RoomGetMessage(size + 5);
            
            msg.Write(RpcUtils.GetHeader(mode, MsgType.Netview));
            Id.OnSerialize(msg);
            msg.Write(cid);
            msg.Write(rpcId);
            return msg;
        }

        /// <summary>
        /// start a message for one specific player
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="rpcId"></param>
        /// <param name="mode"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        NetMessage StartMessage(byte cid, byte rpcId, ReliabilityMode mode, int size)
        {
            var msg = Room.RoomGetMessage(size + 5);

            //just using "server" because its going to one target that isn't the owner
            msg.Write(RpcUtils.GetHeader(mode, BroadcastMode.Server, MsgType.Netview));
            Id.OnSerialize(msg);
            msg.Write(cid);
            msg.Write(rpcId);
            return msg;
        }

        internal void SendMessage(NetMessage msg, RpcMode mode)
        {
            var reliable = mode.ReliabilityMode();
            var broadcast = mode.BroadcastMode();

            if (!Owner.IsValid)
                if (broadcast == BroadcastMode.Others)
                    broadcast = BroadcastMode.All;
                else if (broadcast == BroadcastMode.Owner)
                {
                    NetMessage.RecycleMessage(msg);
                    return;
                }

            ImplSendMessage(msg, reliable, broadcast);
        }

        /// <summary>
        /// Send message. Filtering is already done for invalid broadcast+owner combinations (others+server, owner+server)
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="reliable"></param>
        /// <param name="broadcast"></param>
        void ImplSendMessage(NetMessage msg, ReliabilityMode reliable, BroadcastMode broadcast)
        {
            switch (broadcast)
            {
                case BroadcastMode.All:
                    if (ObserversAndOwner.Count > 0)
                        Manager.Room.SendToConnections(ObserversAndOwner, msg, reliable);
                    break;
                case BroadcastMode.Others:
                    if (Observers.Count > 0)
                        Manager.Room.SendToConnections(Observers, msg, reliable);
                    break;
                case BroadcastMode.Owner:
                    Manager.Room.SendToPlayer(Owner, msg, reliable);
                    break;
                default:
                    throw new NotImplementedException();
                    break;
            }
        }

        internal void SendMessage(NetMessage msg, Player player, ReliabilityMode mode)
        {
            if (!player.IsValid)
            {
                NetMessage.RecycleMessage(msg);
                return;
            }

            Manager.Room.SendToPlayer(player, msg, mode);
        }

        /// <summary>
        /// Send a message to all players except the specified one (same as All if player is Player.Server)
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="player"></param>
        /// <param name="mode"></param>
        internal void SendExcept(NetMessage msg, Player player, ReliabilityMode mode)
        {
            if (!player.IsValid)
                ImplSendMessage(msg, mode, BroadcastMode.All);
            else
                ImplSendExcept(msg, player, mode);
        }

        void ImplSendExcept(NetMessage msg, Player player, ReliabilityMode mode)
        {
            var conns = new List<object>();
// ReSharper disable once ForCanBeConvertedToForeach speed is necessary
            for (int i = 0; i < ObserversAndOwner.Count; i++)
            {
                var c = ObserversAndOwner[i];
                if (c != player.Connection) conns.Add(c);
            }
            if (conns.Count == 0) return;
            Manager.Room.SendToConnections(conns, msg, mode);
        }

        partial void ImplSendMessage(NetMessage msg, List<Player> players, ReliabilityMode mode);
    }
}
