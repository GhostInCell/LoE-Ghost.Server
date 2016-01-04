using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Players;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using Ghost.Server.Utilities.Interfaces;
using PNet;
using PNetR;
using System.Numerics;
using System;

namespace Ghost.Server.Core.Movment
{
    [NetComponent(2)]
    public class PlayerMovement : MovementGenerator, IUpdatable
    {
        private int _update;
        private double _time;
        private bool _locked;
        private bool _flying;
        private bool _running;
        private bool _resetLock;
        private SyncEntry _entry;
        private MapPlayer _player;
        private int _lastAnimation;
        public override int Animation
        {
            get { return _lastAnimation; }
        }
        public override bool IsLocked
        {
            get
            {
                return _locked;
            }
        }
        public override bool IsFlying
        {
            get
            {
                return _flying;
            }
        }
        public override bool IsRunning
        {
            get
            {
                return _running;
            }
        }
        public override bool IsMovable
        {
            get
            {
                return true;
            }
        }
        public PlayerMovement(WO_Player obj) 
            : base(obj)
        {
            _player = obj.Player;
            _entry = new SyncEntry();
            _position = obj.Player.Data.Position;
            _rotation = obj.Player.Data.Rotation;
            _object.OnSpawn += PlayerMovement_OnSpawn;
            _object.OnDespawn += PlayerMovement_OnDespawn;
        }
        public override void Unlock()
        {
            _object.View?.Lock(_locked = false);
        }
        public override void Destroy()
        {
            _locked = true;
            _object.View.ReceivedStream -= View_ReceivedStream;
            _object.View.GettingPosition -= View_GettingPosition;
            _object.View.GettingRotation -= View_GettingRotation;
            _entry = null;
            _object = null;
            _player = null;
        }
        public void Update(TimeSpan time)
        {
            if ((_update -= time.Milliseconds) <= 0)
            {
                _update = _interval;
                var msg = _object.View.CreateStream(_entry.AllocSize);
                _entry.OnSerialize(msg);_object.View.SendStream(msg);
            }
        }
        public override void Lock(bool reset = true)
        {
            _resetLock = reset;
            _object.View?.Lock(_locked = true);
        }
        public override void LookAt(WorldObject obj)
        {
        }
        #region RPC Handlers
        [Rpc(201)]
        private void RPC_02_201(NetMessage arg1, NetMessageInfo arg2)
        {
            _position = arg1.ReadVector3();
            _object.View.Teleport(_position);
        }
        [Rpc(202)]
        private void RPC_02_202(NetMessage arg1, NetMessageInfo arg2)
        {
            _lastAnimation = arg1.ReadInt32();
            _flying = (_player.Char.Pony.Race == 3 && _lastAnimation == 1);
            _object.View.Rpc(2, 202, RpcMode.OthersOrdered, _lastAnimation);
        }
        #endregion
        #region Events Handlers
        private void PlayerMovement_OnSpawn()
        {
            _object.View.SubscribeMarkedRpcsOnComponent(this);
            _object.View.ReceivedStream += View_ReceivedStream;
            _object.View.GettingPosition += View_GettingPosition;
            _object.View.GettingRotation += View_GettingRotation;
        }
        private void PlayerMovement_OnDespawn()
        {
            _object.View.ReceivedStream -= View_ReceivedStream;
            _object.View.GettingPosition -= View_GettingPosition;
            _object.View.GettingRotation -= View_GettingRotation;
        }
        private Vector3 View_GettingRotation()
        {
            return _rotation.ToDegrees();
        }
        private Vector3 View_GettingPosition()
        {
            return _position;
        }
        private void View_ReceivedStream(NetMessage arg1, Player arg2)
        {
            if (_resetLock)
                _object.View.Lock(_resetLock = _locked = false);
            else if (_locked) return;
            _entry.OnDeserialize(arg1);
            float distance = Vector3.Distance(_position, _entry.Position);
            _speed = (float)(distance / (_entry.Time - _time));
            if (distance > 0.01)
            {
                if (_speed > 25f && _player.User.Access < AccessLevel.TeamMember)
                    _player.Player.Disconnect("MOV EAX, #DEADC0DE");
                else
                {
                    switch (_player.Char.Pony.Race)
                    {
                        case 1:
                            _running = _speed > 9.5f;
                            break;
                        case 2:
                            _running = _speed > 8.5f;
                            break;
                        case 3:
                            if (_flying)
                                _running = _speed > 16.5f;
                            else
                                _running = _speed > 8.8f;
                            break;
                    }
                }
                if (_player.Shop != null && Vector3.Distance(_position, _player.Shop.Position) > Constants.MaxInteractionDistance)
                    _player.Shop.CloseShop(_player);
                if (_player.Trade.IsTrading && Vector3.Distance(_position, _player.Trade.Target.Object.Position) > Constants.MaxInteractionDistance)
                    _player.Trade.CloseBoth();
            }
            else _running = false;
            _time = _entry.Time;
            _position = _entry.Position;
            _rotation = _entry.Rotation;
        }
        #endregion
    }
}