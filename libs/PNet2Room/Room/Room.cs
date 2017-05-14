using Lidgren.Network;
using PNet;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PNetR
{
    public partial class Room
    {
        //private bool _shutdownQueued = false;
        private readonly Dictionary<int, Player> _players = new Dictionary<int, Player>();

        public readonly SerializationManager Serializer = new SerializationManager();
        public readonly NetworkViewManager NetworkManager;
        public readonly SceneViewManager SceneViewManager;
        /// <summary>
        /// event fired when a Player object is constructing, should be used to return the object used for Player.NetUserData
        /// </summary>
        public event Func<INetSerializable> ConstructNetData;

        /// <summary>
        /// unique identifier this room has on the server
        /// </summary>
        public Guid RoomId { get; internal set; }
        /// <summary>
        /// connection status to dispatch server
        /// </summary>
        public ConnectionStatus ServerStatus { get; private set; }
        public Server Server { get; internal set; }
        public NetworkConfiguration Configuration { get; private set; }

        public IEnumerable<Player> Players { get { return _players.Values; } }

        public event Action<Player> PlayerAdded;
        public event Action<Player> PlayerRemoved;
        public event Action ServerStatusChanged;

        private List<NetIncomingMessage> m_messages;

        public Room(NetworkConfiguration configuration)
        {
            m_messages = new List<NetIncomingMessage>();
            NetworkManager = new NetworkViewManager(this);
            SceneViewManager = new SceneViewManager(this);
            _players[0] = Player.Server;
            NetComponentHelper.FindNetComponents();

            Configuration = configuration;
            ImplConnectionSetup();
            ImplDispatchSetup();
        }

        /// <summary>
        /// start the server's networking.
        /// </summary>
        public void StartConnection()
        {
            ImplRoomStart();
            ImplDispatchConnect();
        }
        partial void ImplRoomStart();
        partial void ImplDispatchConnect();
        partial void ImplConnectionSetup();
        partial void ImplDispatchSetup();

        public void ReadQueue()
        {
            ImplRoomRead();
            ImplDispatchRead();
        }
        partial void ImplRoomRead();
        partial void ImplDispatchRead();

        public void Shutdown(string reason = "Shutting down")
        {
            //_shutdownQueued = true;
            ImplDispatchDisconnect(reason);
            ImplRoomDisconnect(reason);
        }

        partial void ImplRoomDisconnect(string reason);
        partial void ImplDispatchDisconnect(string reason);

        public Player GetPlayer(int id)
        {
            _players.TryGetValue(id, out var player);
            return player;
        }

        private void OnPlayerRemoved(Player player)
        {
            PlayerRemoved?.Invoke(player);
            foreach (var view in NetworkManager.AllViews)
            {
                if (view == null)
                    continue;
                if (view.Owner == player)
                    view.Owner = player.CopyInvalid();
                view.OnPlayerLeftRoom(player);
            }

            CleanupInvalidNetworkViewOwners();
        }

        void CleanupInvalidNetworkViewOwners()
        {
            foreach (var view in NetworkManager.AllViews)
            {
                if (view == null || !view.Owner.IsValid || _players.ContainsValue(view.Owner)) continue;
                view.Owner = view.Owner.CopyInvalid();
            }
        }

        /// <summary>
        /// Instantiate a network view over the network, as the specified resource
        /// </summary>
        /// <param name="resource">path to the resource to instantiate</param>
        /// <param name="position">starting position</param>
        /// <param name="rotation">starting rotation</param>
        /// <param name="owner">The owner of the networkview. Null is server.</param>
        /// <param name="visCheck">subscribed to NetworkView.CheckVisibility</param>
        /// <returns></returns>
        public NetworkView Instantiate(string resource, Vector3 position, Vector3 rotation, Player owner = null, Func<Player, bool> visCheck = null)
        {
            CleanupInvalidNetworkViewOwners();

            if (owner == null)
                owner = Player.Server;
            else if (!_players.ContainsValue(owner))
            {
                throw new ArgumentException("specified player is no longer connected!", "owner");
            }

            if (resource == null)
                throw new ArgumentNullException("resource");

            var view = NetworkManager.GetNew(owner);
            view.CheckVisibility += visCheck;
            view.Resource = resource;

            var msg = ConstructInstMessage(view, position, rotation);
            //rebuildvis causes msg to be recycled. need to clone it first.
            var omsg = new NetMessage();
            msg.Clone(omsg);
            view.RebuildVisibility(msg);
            //rebuild visiblity skips the owner.
            if (owner.IsValid)
                owner.SendMessage(omsg, ReliabilityMode.Ordered);

            return view;
        }

        internal NetMessage ConstructInstMessage(NetworkView view, Vector3 position, Vector3 rotation)
        {
            var msg = RoomGetMessage(view.Resource.Length * 2 + 34);
            msg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.All, MsgType.Internal));
            msg.Write(RandPRpcs.Instantiate);
            msg.Write(view.Id);
            msg.Write(view.Owner.Id);
            msg.Write(view.Resource);
            msg.Write(position.X);
            msg.Write(position.Y);
            msg.Write(position.Z);
            msg.Write(rotation.X);
            msg.Write(rotation.Y);
            msg.Write(rotation.Z);
            return msg;
        }

        /// <summary>
        /// Destroy view over the network
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public bool Destroy(NetworkView view, byte reasonCode = 0)
        {
            if (!NetworkManager.Contains(view))
                return false;
            view.Destroy();
            NetworkManager.Remove(view);

            var msg = GetDestroyMessage(view, RandPRpcs.Destroy, reasonCode);

            ImplSendToPlayers(msg, ReliabilityMode.Ordered);
            return true;
        }

        internal NetMessage GetDestroyMessage(NetworkView view, byte destType, byte reasonCode = 0)
        {
            var msg = RoomGetMessage(6);
            msg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.All, MsgType.Internal));
            msg.Write(destType);
            msg.Write(view.Id);
            if (reasonCode != 0)
                msg.Write(reasonCode);
            return msg;
        }

        internal NetMessage RoomGetMessage(int size)
        {
            NetMessage msg = null;
            ImplRoomGetMessage(size, ref msg);
            return msg;
        }

        partial void ImplRoomGetMessage(int size, ref NetMessage msg);

        internal NetMessage ServerGetMessage(int size)
        {
            NetMessage msg = null;
            ImplServerGetMessage(size, ref msg);
            return msg;
        }

        partial void ImplServerGetMessage(int size, ref NetMessage msg);

        public string GetWholeState()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Room {0}", RoomId).AppendLine();
            sb.AppendLine("Players:");
            foreach (var player in Players)
            {
                if (player == null) continue;
                sb.AppendFormat("  player: id {0}, {1} - {2}", player.Id, player.UserData, player.NetUserData).AppendLine();
            }

            sb.AppendLine("Network views:");
            foreach (var view in NetworkManager.AllViews)
            {
                if (view == null) continue;
                sb.AppendFormat("  view: id {0} owner {1} - {2} resource {3}", view.Id, view.Owner.Id, view.Owner.NetUserData, view.Resource).AppendLine();
            }

            return sb.ToString();
        }
    }
}