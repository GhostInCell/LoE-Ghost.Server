using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Events;
using Ghost.Server.Core.Movment;
using Ghost.Server.Core.Players;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using Ghost.Server.Mgrs.Map;
using Ghost.Server.Mgrs.Mob;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using Ghost.Server.Utilities.Interfaces;
using Ghost.Server.Utilities.Interfaces.Script;
using System;
using System.Numerics;

namespace Ghost.Server.Core.Objects
{
    public class WO_MOB : CreatureObject, IUpdatable
    {
        private readonly string _resource;
        private readonly DB_WorldObject _data;
        private readonly DB_Creature _creature;
        private float _aTime;
        private WO_Player _target;
        private TargetEntry _tEntry;
        private IScriptedAI _script;
        private AutoRespawn _respawn;
        private MobThreatMgr _threat;
        public WO_Player Target
        {
            get { return _target; }
        }
        public IScriptedAI Script
        {
            get
            {
                return _script;
            }
        }
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
        public MobThreatMgr ThreatMgr
        {
            get
            {
                return _threat;
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
            //if ((_creature.Flags & CreatureFlags.Scripted) > 0)
            //    _script = ScriptsMgr.GetScript<IScriptedAI>((ushort)data.ObjectID);
            _stats = new MobStatsMgr(this);
            _threat = new MobThreatMgr(this);
            _movement = new MobMovement(this);
            _tEntry = new TargetEntry() { SkillID = _creature.SpellID, Upgrade = 0 };
            Spawn();
        }
        public void Update(TimeSpan time)
        {
            if (_aTime > 0) _aTime -= time.Milliseconds;
            if (_threat.SelectTarget(out _target))
                DoMeleeAttackIfReady();
        }
        public void DoMeleeAttackIfReady()
        {
            if (_target != null && _aTime <= 0)
            {
                _aTime = _creature.Attack_Rate;
                if (!_target.IsSpawned)
                {
                    _threat.Remove(_target);
                    _target = null;
                }
                else if (Vector3.Distance(_movement.Position, _target.Position) <= (Constants.MeleeCombatDistance + 0.1f))
                {
                    _view.PerformSkill(_tEntry.Fill(_target));
                    _target.Player.Stats.DoDamage(this, (_stats as MobStatsMgr).RandomDamage);
                    if (!_target.IsSpawned) _threat.Remove(_target);
                }
            }
        }
        public void Kill()
        {
            var player = _threat.Attacker;
            if (player != null)
            {
                player.Player.Stats.AddExp(Talent.Combat, (uint)_stats.Level * 100);
                new WO_Loot(_creature.LootID, this, player.Player, _manager);
            }
            if ((_data.Flags & 1) == 1)
                Despawn();
            else
                Destroy();
        }
        #region Events Handlers
        private void WO_MOB_OnSpawn()
        {
            if (_resource == null) return;
            _respawn?.Destroy(); _respawn = null;
            _stats.UpdateStats();
            _movement.Position = _data.Position;
            _movement.Rotation = _data.Rotation.ToRadians();
            _view = _server.Room.Instantiate(_resource, _data.Position, _data.Rotation);
            if (_script != null) _script.SetOwner(this);
        }
        private void WO_MOB_OnDespawn()
        {
            _aTime = 0f;
            _target = null;
            _threat.Clear();
            _respawn?.Destroy();
            if ((_data.Flags & 1) == 1)
                _respawn = new AutoRespawn(this, _data.Time);
        }
        private void WO_MOB_OnDestroy()
        {
            _respawn?.Destroy();
            _respawn = null;
            _threat.Destroy();
            _threat = null;
            _target = null;
        }
        #endregion
    }
}