using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Objects;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNet;
using PNetR;
using System.Numerics;

namespace Ghost.Server.Core.Movment
{
    [NetComponent(2)]
    public class PetMovement : MovementGenerator
    {
        private double _time;
        private bool _locked;
        private bool _resetLock;
        private SyncEntry _entry;
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
        public PetMovement(WO_Pet obj)
            : base(obj)
        {
            _entry = new SyncEntry();
            _position = obj.SpawnPosition;
            _rotation = obj.SpawnRotation;
            _object.OnSpawn += PetMovement_OnSpawn;
            _object.OnDespawn += PetMovement_OnDespawn;
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
            int animation = arg1.ReadInt32();
            _object.View.Rpc(2, 202, RpcMode.OthersOrdered, animation);
        }
        #endregion
        #region Events Handlers
        private void PetMovement_OnSpawn()
        {
            _object.View.SubscribeMarkedRpcsOnComponent(this);
            _object.View.ReceivedStream += View_ReceivedStream;
            _object.View.GettingPosition += View_GettingPosition;
            _object.View.GettingRotation += View_GettingRotation;
        }
        private void PetMovement_OnDespawn()
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
            if (_locked && _resetLock) _object.View.Lock(_locked = false);
            _entry.OnDeserialize(arg1);
            //float distance = Vector3.Distance(_position, _entry.Position);
            //_speed = (float)(distance / (_entry.Time - _time));
            _time = _entry.Time;
            _position = _entry.Position;
            _rotation = _entry.Rotation;
            var msg = _object.View.CreateStream(_entry.AllocSize);
            _entry.OnSerialize(msg); _object.View.SendStream(msg);

        }
        #endregion
    }
}