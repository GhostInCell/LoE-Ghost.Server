using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using System;
using System.Numerics;

namespace Ghost.Server.Core.Movment
{
    public class NullMovement : MovementGenerator
    {
        public NullMovement(CreatureObject obj) 
            : base(obj)
        {
            _object.OnSpawn += NullMovement_OnSpawn;
            _object.OnDespawn += NullMovement_OnDespawn;
            _position = obj.SpawnPosition;
            _rotation = obj.SpawnRotation;
        }
        public override int Animation
        {
            get { return 0; }
        }
        public override bool IsFlying
        {
            get
            {
                return false;
            }
        }
        public override bool IsLocked
        {
            get
            {
                return true;
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
                return false;
            }
        }
        public override void Unlock()
        {
        }
        public override void Destroy()
        {
            _object.View.GettingPosition -= View_GettingPosition;
            _object.View.GettingRotation -= View_GettingRotation;
            _object = null;
        }
        public override void Lock(bool reset = true)
        {
        }
        public override void LookAt(WorldObject obj)
        {
        }
        #region Events Handlers
        private void NullMovement_OnSpawn()
        {
            _object.View.GettingPosition += View_GettingPosition;
            _object.View.GettingRotation += View_GettingRotation;
        }
        private void NullMovement_OnDespawn()
        {
            _object.View.GettingPosition -= View_GettingPosition;
            _object.View.GettingRotation -= View_GettingRotation;
        }
        private Vector3 View_GettingRotation()
        {
            return _rotation;
        }
        private Vector3 View_GettingPosition()
        {
            return _position;
        }
        #endregion
    }
}