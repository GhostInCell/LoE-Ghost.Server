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
            NetworkedSceneObjectView view;
            if (!_views.TryGetValue(id, out view))
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
            var view = new NetworkedSceneObjectView(Room) {NetworkID = id};
            _views[id] = view;
            return view;
        }
    }
}
