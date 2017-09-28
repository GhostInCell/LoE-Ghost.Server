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
using static Ghost.Server.Utilities.NetConverter;

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
        public PlayerMovement(WO_Player parent) 
            : base(parent)
        {
            _player = parent.Player;
            _entry = new SyncEntry();
            _position = _player.Data.Position;
            _rotation = _player.Data.Rotation;
            parent.OnSpawn += PlayerMovement_OnSpawn;
            parent.OnDestroy += PlayerMovement_OnDestroy;
        }
        public override void Unlock()
        {
            _creature.View?.Lock(_locked = false);
        }
        public void Update(TimeSpan time)
        {
            if ((_update -= time.Milliseconds) <= 0)
            {
                _update = _interval;
                var msg = _creature.View.CreateStream(_entry.AllocSize);
                _entry.Time = _time;
                _entry.Position = _position;
                _entry.Rotation = _rotation;
                _entry.OnSerialize(msg);
                _creature.View.SendStream(msg);
            }
        }
        public override void Lock(bool reset = true)
        {
            _resetLock = reset;
            _creature.View?.Lock(_locked = true);
        }
        public override void LookAt(WorldObject obj)
        {
        }
        public override void Teleport(Vector3 position)
        {
            _position = position;
            _time = PNet.Utilities.Now * 1.0005;
        }
        #region RPC Handlers
        [Rpc(201)]
        private void RPC_02_201(NetMessage arg1, NetMessageInfo arg2)
        {
            _position = arg1.ReadVector3();
            _creature.View.Teleport(_position);
        }
        [Rpc(202)]
        private void RPC_02_202(NetMessage arg1, NetMessageInfo arg2)
        {
            _lastAnimation = arg1.ReadInt32();
            _flying = (_player.Char.Pony.Race == CharacterType.Pegasus && _lastAnimation == 1);
            _creature.View.Rpc<Int32Serializer>(2, 202, RpcMode.OthersOrdered, _lastAnimation);
        }
        #endregion
        #region Events Handlers
        private void PlayerMovement_OnSpawn()
        {
            _creature.View.SubscribeMarkedRpcsOnComponent(this);
            _creature.View.ReceivedStream += View_ReceivedStream;
            _creature.View.GettingPosition += View_GettingPosition;
            _creature.View.GettingRotation += View_GettingRotation;
        }
        private void PlayerMovement_OnDestroy()
        {
            _player.Data.Position = _position;
            _player.Data.Rotation = _rotation;
            _entry = null;
            _player = null;
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
                _creature.View.Lock(_resetLock = _locked = false);
            else if (_locked) return;
            _entry.OnDeserialize(arg1);
            if (_time < _entry.Time)
            {
                float distance = Vector3.DistanceSquared(_position, _entry.Position);
                _speed = distance / (float)(_entry.Time - _time);
                if (distance > Constants.EpsilonX2)
                {
                    //if (_speed > 45f && _player.User.Access < AccessLevel.TeamMember)
                    //{
                        //_locked = true;
                        //_player.Disconnect("MOV EAX, #DEADC0DE");
                    //}
                    //else
                    {
                        switch (_player.Char.Pony.Race)
                        {
                            case CharacterType.EarthPony:
                                _running = _speed > 6f;
                                break;
                            case CharacterType.Unicorn:
                                _running = _speed > 5.15f;
                                break;
                            case CharacterType.Pegasus:
                                if (_flying)
                                    _running = _speed > 18f;
                                else
                                    _running = _speed > 5.25f;
                                break;
                        }
                    }
                    if (_player.Shop != null && Vector3.DistanceSquared(_position, _player.Shop.Position) > Constants.MaxInteractionDistanceSquared)
                        _player.Shop.CloseShop(_player);
                    if (_player.Trade.IsTrading && Vector3.DistanceSquared(_position, _player.Trade.Target.Object.Position) > Constants.MaxInteractionDistanceSquared)
                        _player.Trade.CloseBoth();
                }
                else _running = false;
                _time = _entry.Time;
                _position = _entry.Position;
                _rotation = _entry.Rotation;
            }
        }
        #endregion
    }
}