#if R_LIDGREN
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using System;
using PNet;

namespace PNetR
{
    public partial class NetworkView
    {
        private NetConnection _ownerConn;
        readonly List<NetConnection> _observers = new List<NetConnection>();
        readonly List<NetConnection> _observersAndOwner = new List<NetConnection>();

        partial void ImplSendMessage(NetMessage msg, ReliabilityMode reliable, BroadcastMode broadcast)
        {
            var lmsg = Manager.Room.PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            int seq;
            var method = reliable.PlayerDelivery(out seq);

            switch (broadcast)
            {
                case BroadcastMode.All:
                    if (_observersAndOwner.Count > 0)
                        Manager.Room.PlayerServer.SendMessage(lmsg, _observersAndOwner, method, seq);
                    break;
                case BroadcastMode.Others:
                    if (_observers.Count > 0)
                        Manager.Room.PlayerServer.SendMessage(lmsg, _observers, method, seq);
                    break;
                case BroadcastMode.Owner:
                    Manager.Room.PlayerServer.SendMessage(lmsg, Owner.Connection, method, seq);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        partial void ImplSendMessage(NetMessage msg, ReliabilityMode mode, Player player)
        {
            var lmsg = Manager.Room.PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            int seq;
            var method = mode.PlayerDelivery(out seq);
            Manager.Room.PlayerServer.SendMessage(lmsg, player.Connection, method, seq);
        }

        partial void ImplSendExcept(NetMessage msg, Player player, ReliabilityMode mode)
        {
            var lmsg = Manager.Room.PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            int seq;
            var method = mode.PlayerDelivery(out seq);
            var conns = _observersAndOwner.Where(c => c != player.Connection).ToArray();
            if (conns.Length == 0) return;
            Manager.Room.PlayerServer.SendMessage(lmsg, conns, method, seq);
        }

        partial void ImplSendMessage(NetMessage msg, List<Player> players, ReliabilityMode mode)
        {
            var lmsg = Manager.Room.PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            int seq;
            var method = mode.PlayerDelivery(out seq);
            var conns = new List<NetConnection>(players.Count);
// ReSharper disable once LoopCanBeConvertedToQuery  - speed
            foreach (var p in players)
            {
                conns.Add(p.Connection);
            }
            if (conns.Count == 0)
                return;
            Manager.Room.PlayerServer.SendMessage(lmsg, conns, method, seq);
        }

        partial void ImplObservePlayer(Player player)
        {
            _observers.Add(player.Connection);
            _observersAndOwner.Add(player.Connection);
        }

        partial void ImplIgnorePlayer(Player player)
        {
            _observers.Remove(player.Connection);
            _observersAndOwner.Remove(player.Connection);
        }

        partial void ImplOwnerChanged()
        {
            _observersAndOwner.Remove(_ownerConn);
            if (!Owner.IsValid) return;
            _ownerConn = Owner.Connection;
            _observersAndOwner.Add(_ownerConn);
        }
    }
}
#endif