using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Objects;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using Ghost.Server.Utilities.Interfaces;
using System;
using System.Numerics;

namespace Ghost.Server.Core.Movment
{
    public class MobMovement : MovementGenerator, IUpdatable
    {
        private int _update;
        private bool _locked;
        private SyncEntry _entry;
        private ScriptedAI _scriptedAI;
        public override int Animation
        {
            get { return 0; }
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
                return false;
            }
        }
        public override bool IsRunning
        {
            get
            {
                return false;
            }
        }
        public override bool IsMovable
        {
            get
            {
                return true;
            }
        }
        public MobMovement(WO_MOB parent) 
            : base(parent)
        {
            _entry = new SyncEntry();
            _position = _creature.SpawnPosition;
            _rotation = _creature.SpawnRotation.ToRadians();
            parent.OnSpawn += MobMovement_OnSpawn;
            parent.OnDestroy += MobMovement_OnDestroy;
            parent.OnInitialize += MobMovement_OnInitialize;
        }
        public override void Unlock()
        {
            _locked = false;
        }
        public void Update(TimeSpan time)
        {
            if (!_locked)
            {
                if (!(_scriptedAI.Target?.IsDead ?? true))
                {
                    _direction = Vector3.Normalize(_scriptedAI.Target.Position - _position);
                    _rotation = MathHelper.GetRotation(Vector3.UnitZ, _direction, Vector3.UnitY);
                    if (Vector3.Distance(_position, _scriptedAI.Target.Position) > Constants.MeleeCombatDistance)
                    {
                        var offset = (_direction * ((time.Milliseconds / 1000f) * _speed / 45f));
                        if (Vector3.Distance(_position, _scriptedAI.Target.Position) > Constants.MeleeCombatDistance + offset.Length())
                            _position += offset;
                        else
                            _position = _scriptedAI.Target.Position - (_direction * (Constants.MeleeCombatDistance - 0.05f));
                    }
                }
                else if (Vector3.Distance(_position, _creature.SpawnPosition) > 0)
                {
                    _direction = Vector3.Normalize(_creature.SpawnPosition - _position);
                    _rotation = MathHelper.GetRotation(Vector3.UnitZ, _direction, Vector3.UnitY);
                    var offset = (_direction * ((time.Milliseconds / 1000f) * _speed / 45f));
                    if (Vector3.Distance(_position, _creature.SpawnPosition) > offset.Length())
                        _position += offset;
                    else
                        _position = _creature.SpawnPosition;
                }
            }
            if ((_update -= time.Milliseconds) <= 0)
            {
                _update = _interval;
                _entry.Position = _position;
                _entry.Rotation = _rotation;
                _entry.Time = PNet.Utilities.Now;
                var msg = _creature.View.CreateStream(_entry.AllocSize);
                _entry.OnSerialize(msg); _creature.View.SendStream(msg);
            }
        }
        public override void Lock(bool reset = true)
        {
            _locked = true;
        }
        public override void LookAt(WorldObject obj)
        {
        }
        #region Events Handlers
        private void MobMovement_OnSpawn()
        {
            _speed = _creature.Stats.Speed;
            _creature.View.GettingPosition += View_GettingPosition;
            _creature.View.GettingRotation += View_GettingRotation;
        }
        private void MobMovement_OnDestroy()
        {
            _entry = null;
            _scriptedAI = null;
        }
        private void MobMovement_OnInitialize()
        {
            _scriptedAI = _parent.RequiredComponent<ScriptedAI>();
        }
        private Vector3 View_GettingRotation()
        {
            return _rotation.ToDegrees();
        }
        private Vector3 View_GettingPosition()
        {
            return _position;
        }
        #endregion
    }
}