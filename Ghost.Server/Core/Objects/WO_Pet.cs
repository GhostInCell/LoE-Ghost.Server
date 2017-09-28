using Ghost.Server.Core.Movment;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNet;
using PNetR;
using System.Numerics;
using static Ghost.Server.Utilities.NetConverter;

namespace Ghost.Server.Core.Objects
{
    public class WO_Pet : CreatureObject
    {
        private readonly string _resource;
        private readonly DB_Creature _creature;
        private WO_Player _owner;
        public int ID
        {
            get { return _creature.ID; }
        }
        public override byte TypeID
        {
            get
            {
                return Constants.TypeIDPet;
            }
        }
        public override Vector3 SpawnPosition
        {
            get
            {
                return _owner.Position;
            }
        }
        public override Vector3 SpawnRotation
        {
            get
            {
                return _owner.Rotation;
            }
        }
        public WO_Pet(int id, WO_Player owner)
            : base(owner.Manager.GetNewGuid() | Constants.DRObject, owner.Manager)
        {
            _owner = owner;
            if (!DataMgr.Select(id, out _creature))
                ServerLogger.LogError($"Creature id {id} doesn't exist");
            else if (string.IsNullOrEmpty(_resource = DataMgr.SelectResource(_creature.Resource)))
                ServerLogger.LogError($"Resource id {_creature.Resource} doesn't exist");
            OnSpawn += WO_Pet_OnSpawn;
            OnDestroy += WO_Pet_OnDestroy;
            AddComponent(new PetMovement(this));
            Spawn();
        }
        #region RPC Handlers
        private void RPC_04_53(NetMessage arg1, NetMessageInfo arg2)
        {
            _view.Rpc<Int16Serializer>(4, 53, arg2.Sender, _owner.Stats.Level);
        }
        #endregion
        #region Events Handlers
        private void WO_Pet_OnSpawn()
        {
            if (_resource == null) return;
            _view = _server.Room.Instantiate(_resource, _owner.Position, _owner.Rotation, _owner.Player.Player);
            _view.SubscribeToRpc(4, 53, RPC_04_53);
        }
        private void WO_Pet_OnDestroy()
        {
            _owner = null;
        }
        #endregion
    }
}