using Ghost.Server.Core.Events;
using Ghost.Server.Core.Movment;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using Ghost.Server.Mgrs.Map;
using Ghost.Server.Mgrs.Mob;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNetR;
using System.Numerics;
using static PNet.NetConverter;

namespace Ghost.Server.Core.Objects
{
    public class WO_MOB : CreatureObject
    {
        private readonly string _resource;
        private readonly DB_WorldObject _data;
        private readonly DB_Creature _creature;
        private AutoRespawn _respawn;
        private MobThreatMgr _threat;
        public DB_WorldObject Data
        {
            get
            {
                return _data;
            }
        }
        public DB_Creature Creature
        {
            get
            {
                return _creature;
            }
        }
        public override byte TypeID
        {
            get
            {
                return Constants.TypeIDMOB;
            }
        }
        public override Vector3 SpawnPosition
        {
            get
            {
                return _data.Position;
            }
        }
        public override Vector3 SpawnRotation
        {
            get
            {
                return _data.Rotation;
            }
        }
        public WO_MOB(DB_WorldObject data, ObjectsMgr manager)
            : base(manager.GetNewGuid() | Constants.ReleaseGuide, manager)
        {
            _data = data;
            if (!DataMgr.Select(data.ObjectID, out _creature))
                ServerLogger.LogError($"Creature id {data.ObjectID} doesn't exist");
            else if (string.IsNullOrEmpty(_resource = DataMgr.SelectResource(_creature.Resource)))
                ServerLogger.LogError($"Resource id {_creature.Resource} doesn't exist");
            OnSpawn += WO_MOB_OnSpawn;
            OnDespawn += WO_MOB_OnDespawn;
            OnDestroy += WO_MOB_OnDestroy;
            OnInitialize += WO_MOB_OnInitialize;
            OnKilled += WO_MOB_OnKilled;
            AddComponent(new BasicAI(this));
            AddComponent(new MobStatsMgr(this));
            AddComponent(new MobMovement(this));
            AddComponent(new MobThreatMgr(this));
            Spawn();
        }
        #region Events Handlers
        private void WO_MOB_OnSpawn()
        {
            if (_resource == null) return;
            _respawn?.Destroy();
            _stats.UpdateStats();
            _movement.Position = _data.Position;
            _movement.Rotation = _data.Rotation.ToRadians();
            _view = _server.Room.Instantiate(_resource, _data.Position, _data.Rotation);
            _view.FinishedInstantiation += View_FinishedInstantiation;
        }
        private void WO_MOB_OnDespawn()
        {
            if ((_data.Flags & 1) == 1)
                _respawn = new AutoRespawn(this, _data.Time);
        }
        private void WO_MOB_OnDestroy()
        {
            _respawn?.Destroy();
            _threat = null;
            _respawn = null;
        }
        private void WO_MOB_OnInitialize()
        {
            _threat = RequiredComponent<MobThreatMgr>();
        }
        private void WO_MOB_OnKilled(CreatureObject obj)
        {
            var awards = _threat.ToAward;
            if (awards.Length > 0)
            {
                var bonus = (uint)(_stats.Level * (awards.Length - 1));
                if (_creature.LootID > 0)
                    new WO_Loot(_creature.LootID, this, awards[0].Player, _manager);
                awards[0].Player.Stats.AddExp(TalentMarkId.Combat, (uint)_stats.Level * 25, bonus);
                for (uint i = 1; i < awards.Length; i++)
                    awards[i].Player.Stats.AddExp(TalentMarkId.Combat, (uint)(_stats.Level * 25f / i), bonus / i);
            }
            if ((_data.Flags & 1) == 1)
                Despawn();
            else
                Destroy();
        }
        private void View_FinishedInstantiation(Player obj)
        {
            _view.Rpc<Int32Serializer>(4, 54, obj, _stats.Team);
        }
        #endregion
    }
}