using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Events;
using Ghost.Server.Core.Movment;
using Ghost.Server.Core.Players;
using Ghost.Server.Mgrs.Player;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNetR;
using System;
using System.Numerics;
using static PNet.NetConverter;

namespace Ghost.Server.Core.Objects
{
    public class WO_Player : CreatureObject
    {
        private static readonly TimeSpan respTime = TimeSpan.FromSeconds(Constants.PlayerRespawnTime);
        private MapPlayer _player;
        private AutoRespawn _respawn;
        public MapPlayer Player
        {
            get
            {
                return _player;
            }
        }
        public override byte TypeID
        {
            get
            {
                return Constants.TypeIDPlayer;
            }
        }
        public override Vector3 SpawnPosition
        {
            get
            {
                return _player.Data.Position;
            }
        }
        public override Vector3 SpawnRotation
        {
            get
            {
                return _player.Data.Rotation.ToRadians();
            }
        }
        public WO_Player(MapPlayer player)
            : base(Constants.PlayerObject | player.Player.Id, player.Server.Objects)
        {
            _player = player;
            OnSpawn += WO_Player_OnSpawn;
            OnKilled += WO_Player_OnKilled;
            OnDestroy += WO_Player_OnDestroy;
            OnDespawn += WO_Player_OnDespawn;
            OnInitialize += WO_Player_OnInitialize;
            AddComponent(new ItemsMgr(this));
            AddComponent(new TradeMgr(this));
            AddComponent(new SkillsMgr(this));
            AddComponent(new PlayerMovement(this));
            AddComponent(new PlayerStatsMgr(this));
            Spawn();
        }
        public void Teleport(Vector3 position)
        {
            _view.Teleport(_movement.Position = position);
        }
        #region Events Handlers
        private void WO_Player_OnSpawn()
        {
            _view = _server.Room.Instantiate("PlayerBase", _movement.Position, _movement.Rotation.ToDegrees(), _player.Player);
            _view.FinishedInstantiation += View_FinishedInstantiation;
        }
        private void WO_Player_OnDespawn()
        {
            _respawn?.Destroy();
            _respawn = new AutoRespawn(this, respTime);
            _manager.SetPosition(this, _player.User.Spawn);
            _player.Announce(Constants.DeadMsg, 8f);
        }
        private void WO_Player_OnDestroy()
        {
            _respawn?.Destroy(); 
            _player = null;
            _respawn = null;
        }
        private void WO_Player_OnInitialize()
        {
            if (_player.User.Map != 0 && _player.User.Spawn != 0)
                _manager.SetPosition(this, _player.User.Spawn);
            else if (_manager.MapID != _player.Char.Map)
                _manager.SetPosition(this);
            _player.Data.Position = _movement.Position;
            _player.Data.Rotation = _movement.Rotation;
        }
        private void WO_Player_OnKilled(CreatureObject obj)
        {
            _view.FaintPony();
        }
        private void View_FinishedInstantiation(Player obj)
        {
            _view.Rpc<Int32Serializer>(4, 54, obj, _stats.Team);
            _view.Rpc(7, 4, obj, _player.Data.SerWears);
            if (obj.Id == _player.Player.Id)
            {
                _view.SetBits(_player.Data.Bits);
                _view.Rpc(2, 200, _player.Player, _player.Char);
                _player.Player.Rpc(4, _player.Data.SerTalents);
                _view.Rpc(5, 195, _player.Player, _player.Data.SerSkills);
                _view.Rpc(7, 5, _player.Player, _player.Data.SerInventory);
            }
            else
            {
                _view.Rpc(2, 200, obj, _player.Char);
                _view.Rpc<Int32Serializer>(2, 202, obj, _movement.Animation);
            }
        }
        #endregion
    }
}