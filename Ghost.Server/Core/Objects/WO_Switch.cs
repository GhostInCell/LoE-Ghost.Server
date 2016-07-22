using Ghost.Server.Core.Players;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using Ghost.Server.Mgrs.Map;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNet;
using PNetR;
using System.Numerics;

namespace Ghost.Server.Core.Objects
{
    public class WO_Switch : ServerObject
    {
        private readonly DB_Map _map;
        private NetworkedSceneObjectView _view;
        public override byte TypeID
        {
            get
            {
                return Constants.TypeIDSwitch;
            }
        }
        public WO_Switch(DB_WorldObject data, ObjectsMgr manager)
            : base(data, manager)
        {
            if (!DataMgr.Select(data.Data01, out _map))
                ServerLogger.LogError($"Map Switch {data.Guid} map {data.Data01} doesn't exist");
            else
            {
                _view = _server.Room.SceneViewManager.CreateNetworkedSceneObjectView(_data.Guid);
                _view.SubscribeToRpc(1, RPC_001);
                OnDestroy += WO_Switch_OnDestroy;
            }
            Spawn();
        }
        #region Events Handlers
        private void WO_Switch_OnDestroy()
        {
            _view.UnsubscribeFromRpc(1);
            _view = null;
        }
        #endregion
        private void RPC_001(NetMessage arg1, NetMessageInfo arg2)
        {
            MapPlayer player = _server[arg2.Sender.Id];
            if (Vector3.DistanceSquared(_data.Position, player.Object.Position) <= Constants.MaxInteractionDistanceSquared)
            {
                player.User.Spawn = (ushort)_data.Data02;
                arg2.Sender.SynchNetData();
                arg2.Sender.ChangeRoom(_map.Name);
                player = null;
            }
        }
    }
}