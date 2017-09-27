using Ghost.Server.Core.Events;
using Ghost.Server.Core.Players;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using Ghost.Server.Mgrs.Map;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNet;
using PNetR;
using System;
using System.Numerics;

namespace Ghost.Server.Core.Objects
{
    public class WO_Pickup : WorldObject
    {
        private readonly DB_Item _item;
        private readonly string _resource;
        private readonly DB_WorldObject _data;
        private NetworkView _view;
        private AutoRespawn _respawn;
        public NetworkView View
        {
            get { return _view; }
        }
        public override byte TypeID
        {
            get
            {
                return Constants.TypeIDPickup;
            }
        }
        public override ushort SGuid
        {
            get
            {
                return _view.Id;
            }
        }
        public override Vector3 Position
        {
            get { return _data.Position; }
            set { throw new NotSupportedException(); }
        }
        public override Vector3 Rotation
        {
            get { return _data.Rotation; }
            set { throw new NotSupportedException(); }
        }
        public WO_Pickup(DB_WorldObject data, ObjectsMgr manager)
            : base(manager.GetNewGuid() | Constants.IRObject, manager)
        {
            _data = data;
            if (data.Data01 <= 0)
                ServerLogger.LogWarning($"Pickup {data.Guid} on map {data.Map} has negative or zero amount {data.Data01} of {data.ObjectID}");
            if ((_data.Flags & 1) == 1 && data.Time.TotalSeconds <= 0)
            {
                ServerLogger.LogWarning($"Pickup {data.Guid} on map {data.Map} has respawn flag but negative or zero respawn time {data.Time}");
                _data.Flags &= 254;
            }
            if (!DataMgr.Select(data.ObjectID, out _item))
                ServerLogger.LogError($"Item id {data.ObjectID} doesn't exist");
            _resource = DataMgr.SelectResource(data.Data02);
            OnSpawn += WO_Pickup_OnSpawn;
            OnDespawn += WO_Pickup_OnDespawn;
            OnDestroy += WO_Pickup_OnDestroy;
            Spawn();
        }
        #region RPC Handlers
        private void RPC_50_52(NetMessage arg1, NetMessageInfo arg2)
        {
            MapPlayer player = _server[arg2.Sender.Id];
            if (_view != null && Vector3.DistanceSquared(player.Object.Position, _data.Position) <= Constants.MaxInteractionDistanceSquared)
            {
                if ((_data.Flags & 1) == 1)
                {
                    Despawn();
                    _respawn = new AutoRespawn(this, _data.Time);
                }
                else Destroy();
                player.Items.AddItems(_data.ObjectID, _data.Data01);
            }
        }
        #endregion
        #region Events Handlers
        private void WO_Pickup_OnSpawn()
        {
            if (_resource == null) return;
            _respawn?.Destroy(); _respawn = null;
             _view = _server.Room.Instantiate(_resource, _data.Position, _data.Rotation);
            _view.SubscribeToRpc(50, 52, RPC_50_52);
            _view.GettingPosition += View_GettingPosition;
            _view.GettingRotation += View_GettingRotation;
        }
        private void WO_Pickup_OnDespawn()
        {
            _respawn?.Destroy(); _respawn = null;
            _view.ClearSubscriptions();
            _server.Room.Destroy(_view);
        }
        private void WO_Pickup_OnDestroy()
        {
            _respawn?.Destroy();
            _view.ClearSubscriptions();
            _server.Room.Destroy(_view);
            _view = null;
            _respawn = null;
        }
        private Vector3 View_GettingRotation()
        {
            return _data.Rotation;
        }
        private Vector3 View_GettingPosition()
        {
            return _data.Position;
        }
        #endregion
    }
}