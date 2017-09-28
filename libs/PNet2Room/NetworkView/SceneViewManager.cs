using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PNet;

namespace PNetR
{
    public class SceneViewManager
    {
        public readonly Room Room;
        /// <summary>
        /// For rpcs that are not handled by a subscribed method, either allow or disallow them to continue forwarding
        /// </summary>
        public bool AllowUnhandledRpcForwarding { get; set; }

        internal SceneViewManager(Room room)
        {
            Room = room;
        }

        internal void CallRpc(NetMessage msg, NetMessageInfo info)
        {
            if (msg.RemainingBits < 24L)
            {
                Debug.LogError("Malformed networked scene object rpc");
            }
            var id = msg.ReadUInt16();
            if (!_views.TryGetValue(id, out var view))
            {
                Debug.LogWarning($"Could not find networked scene object {id} to call an rpc on");
                return;
            }
            view.CallRpc(msg.ReadByte(), msg, info);
        }

        private readonly Dictionary<ushort, NetworkedSceneObjectView> _views = new Dictionary<ushort, NetworkedSceneObjectView>();

        /// <summary>
        /// Create a new object with the specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public NetworkedSceneObjectView CreateNetworkedSceneObjectView(ushort id)
        {
            var view = new NetworkedSceneObjectView(this, Room) {NetworkID = id};
            _views[id] = view;
            return view;
        }
    }
}
