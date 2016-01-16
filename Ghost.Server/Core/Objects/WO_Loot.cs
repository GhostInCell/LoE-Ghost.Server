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
    public class WO_Loot : WorldObject
    {
        private readonly DB_Loot _loot;
        private readonly string _resource;
        private MapPlayer _onwer;
        private Vector3 _position;
        private Vector3 _rotation;
        private NetworkView _view;
        private AutoDestroy _destroy;
        public NetworkView View
        {
            get { return _view; }
        }
        public override byte TypeID
        {
            get
            {
                return Constants.TypeIDLoot;
            }
        }
        public override ushort SGuid
        {
            get
            {
                return (ushort)(_guid & 0xFFFF);
            }
        }
        public override Vector3 Position
        {
            get { return _position; }
            set { throw new NotSupportedException(); }
        }
        public override Vector3 Rotation
        {
            get { return _rotation; }
            set { throw new NotSupportedException(); }
        }
        public WO_Loot(int id, WorldObject at, MapPlayer onwer, ObjectsMgr manager)
            : base(manager.GetNewGuid() | Constants.IDRObject, manager)
        {
            _onwer = onwer;
            _position = at.Position;
            _rotation = at.Rotation;
            _loot = DataMgr.SelectLoot(id);
            _resource = DataMgr.SelectResource(Constants.LootResource);
            OnSpawn += WO_Loot_OnSpawn;
            OnDespawn += WO_Loot_OnDespawn;
            OnDestroy += WO_Loot_OnDestroy;
            Spawn();
        }
        #region RPC Handlers
        private void RPC_50_52(NetMessage arg1, NetMessageInfo arg2)
        {
            if (arg2.Sender.Id != _onwer.Player.Id) return;
            if (_view != null && Vector3.Distance(_onwer.Object.Position, _position) <= Constants.MaxInteractionDistance)
            {
                foreach (var item in _loot.Loot)
                {
                    if (item.Item4 == -1f || Constants.RND.NextDouble() <= item.Item4)
                    {
                        if (item.Item1 == -1)
                            _onwer.Items.AddBits(Constants.RND.Next(item.Item2, item.Item3));
                        else
                            _onwer.Items.AddItems(item.Item1, Constants.RND.Next(item.Item2, item.Item3));
                    }
                }
                Destroy();
            }
        }
        #endregion
        #region Events Handlers
        private void WO_Loot_OnSpawn()
        {
            if (_resource == null) return;
            _view = _server.Room.Instantiate(_resource, _position, _rotation);
            _view.SubscribeToRpc(50, 52, RPC_50_52);
            _view.GettingPosition += View_GettingPosition;
            _view.GettingRotation += View_GettingRotation;
            _view.CheckVisibility += View_CheckVisibility;
            _destroy = new AutoDestroy(this, TimeSpan.FromSeconds(Constants.LootDespawnTime));
        }
        private void WO_Loot_OnDespawn()
        {
            _destroy?.Destroy(); _destroy = null;
            _view.ClearSubscriptions();
            _server.Room.Destroy(_view);
        }
        private void WO_Loot_OnDestroy()
        {
            _destroy?.Destroy();
            _view.ClearSubscriptions();
            _server.Room.Destroy(_view);
            _view = null;
            _onwer = null;
            _destroy = null;
        }
        private Vector3 View_GettingRotation()
        {
            return _rotation;
        }
        private Vector3 View_GettingPosition()
        {
            return _position;
        }
        private bool View_CheckVisibility(Player arg)
        {
            return arg.Id == _onwer.Player.Id;
        }
        #endregion
    }
}