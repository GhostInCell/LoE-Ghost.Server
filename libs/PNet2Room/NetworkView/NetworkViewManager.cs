using PNet;

namespace PNetR
{
    public class NetworkViewManager
    {
        readonly IntDictionary<NetworkView> _networkViews = new IntDictionary<NetworkView>();

        /// <summary>
        /// an array of all network views. Null checks are required, as empty keys will be null.
        /// </summary>
        public NetworkView[] AllViews
        {
            get
            {
                lock (_networkViews)
                {
                    return _networkViews.Values;
                }
            }
        }

        /// <summary>
        /// For rpcs that are not handled by a subscribed method, either allow or disallow them to continue forwarding
        /// </summary>
        public bool AllowUnhandledRpcForwarding { get; set; }

        public readonly Room Room;

        internal NetworkViewManager(Room room)
        {
            Room = room;
        }

        internal NetworkView GetNew(Player owner)
        {
            lock (_networkViews)
            {
                var nid = _networkViews.Add(null);
                var view = new NetworkView(this, new NetworkViewId((ushort) nid), owner);
                _networkViews[nid] = view;
                return view;
            }
        }

        public NetworkView Get(NetworkViewId id)
        {
            _networkViews.TryGetValue(id.Id, out var view);
            return view;
        }

        /// <summary>
        /// Checks if the specified object is in the manager. Verifies by id and reference.
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public bool Contains(NetworkView view)
        {
            if (_networkViews.TryGetValue(view.Id.Id, out var oview))
                return oview == view;
            return false;
        }

        /// <summary>
        /// remove the network view. This does not verify that the object in the slot was the same thing. Use contains to check that.
        /// </summary>
        /// <param name="view"></param>
        internal void Remove(NetworkView view)
        {
            _networkViews.Remove(view.Id.Id);
        }

        internal void CallRpc(NetMessage msg, NetMessageInfo info, SubMsgType sub)
        {
            if (msg.RemainingBits < 32)
            {
                Debug.LogWarning("Attempted to call an rpc on a network view, but there weren't enough bits remaining to do it.");
                return;
            }
            var id = msg.ReadUInt16();
            var comp = msg.ReadByte();
            var rpc = msg.ReadByte();

            var supposedToHave = _networkViews.TryGetValue(id, out var view);
            if (supposedToHave && view != null)
                view.IncomingRpc(comp, rpc, msg, info, sub);
            else
            {
                Debug.LogWarning($"Could not find view {id} to call {comp} rpc {rpc}");
            }

            //todo: filter if rpc mode is all/others/owner, and then send to appropriate people.
            if (info.ContinueForwarding && view != null)
            {
                if (info.Mode == BroadcastMode.Others)
                {
                    view.SendExcept(msg, info.Sender, info.Reliability);
                }
                else if (info.Mode == BroadcastMode.All)
                {
                    view.SendMessage(msg, RpcUtils.RpcMode(info.Reliability, info.Mode));
                }
                else if (info.Mode == BroadcastMode.Owner && info.Sender != view.Owner && view.Owner.IsValid)
                {
                    view.Owner.SendMessage(msg, info.Reliability);
                }
            }
        }

        public void Stream(NetMessage msg, Player sender)
        {
            if (msg.RemainingBits < 16)
            {
                Debug.LogWarning("Attempted to read stream for an rpc but there weren't enough bits to do it");
                return;
            }
            var id = msg.ReadUInt16();
            if (_networkViews.TryGetValue(id, out var view))
                view.IncomingStream(msg, sender);
            else
            {
                //Debug.LogWarning($"Could not find view {id} to stream to");
            }
        }

        public void FinishedInstantiate(Player player, NetMessage msg)
        {
            if (msg.RemainingBits < 16)
            {
                Debug.LogError("Malformed finished instantiate");
                return;
            }

            var id = msg.ReadUInt16();
            if (_networkViews.TryGetValue(id, out var view))
            {
                view.OnFinishedInstantiate(player);
            }
        }
    }
}