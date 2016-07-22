using Ghost.Server.Core.Classes;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using Ghost.Server.Utilities.Interfaces;
using PNet;
using PNetR;
using System;
using System.Numerics;

namespace Ghost.Server.Core.Movment
{
    [NetComponent(2)]
    public class PetMovement : MovementGenerator, IUpdatable
    {
        private int _update;
        private SyncEntry _entry;
        public override int Animation
        {
            get { return 0; }
        }
        public override bool IsLocked
        {
            get
            {
                return false;
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
        public PetMovement(CreatureObject parent)
            : base(parent)
        {
            _entry = new SyncEntry();
            _position = _creature.SpawnPosition;
            _rotation = _creature.SpawnRotation;
            parent.OnSpawn += PetMovement_OnSpawn;
            parent.OnDestroy += PetMovement_OnDestroy;
        }
        public override void Unlock()
        {
        }
        public void Update(TimeSpan time)
        {
            if ((_update -= time.Milliseconds) <= 0)
            {
                _update = _interval;
                var msg = _creature.View.CreateStream(_entry.AllocSize);
                _entry.OnSerialize(msg); _creature.View.SendStream(msg);
            }
        }
        public override void Lock(bool reset = true)
        {
        }
        public override void LookAt(WorldObject obj)
        {
        }
        #region Events Handlers
        private void PetMovement_OnSpawn()
        {
            _creature.View.ReceivedStream += View_ReceivedStream;
            _creature.View.GettingPosition += View_GettingPosition;
            _creature.View.GettingRotation += View_GettingRotation;
        }
        private void PetMovement_OnDestroy()
        {
            _entry = null;
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
            _entry.OnDeserialize(arg1);
            _position = _entry.Position;
            _rotation = _entry.Rotation;
        }
        #endregion
    }
}