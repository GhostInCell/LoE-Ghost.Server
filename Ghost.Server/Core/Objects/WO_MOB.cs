using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Events;
using Ghost.Server.Core.Movment;
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
            lock (_resource)
            {
                if (_isKilled) return;
                _isKilled = true;
            }
            var awards = _threat.ToAward;
            if (awards.Length > 0)
            {
                var bonus = (uint)(_stats.Level * (awards.Length - 1));
                if (_creature.LootID > 0)
                    new WO_Loot(_creature.LootID, this, awards[0].Player, _manager);
                awards[0].Player.Stats.AddExp(Talent.Combat, (uint)_stats.Level * 25, bonus);
                for (uint i = 1; i < awards.Length; i++)
                    awards[i].Player.Stats.AddExp(Talent.Combat, (uint)(_stats.Level * 25f / i), bonus / i);
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
            _isKilled = false;
            _stats.UpdateStats();
            _movement.Position = _data.Position;
            _movement.Rotation = _data.Rotation.ToRadians();
            _view = _server.Room.Instantiate(_resource, _data.Position, _data.Rotation);
            _view.FinishedInstantiation += View_FinishedInstantiation;
            if (_script != null) _script.SetOwner(this);
        }
        private void WO_MOB_OnDespawn()
        {
            _view.FinishedInstantiation -= View_FinishedInstantiation;
            _aTime = 0f;
            _target = null;
            _threat.Clear();
            _respawn?.Destroy();
            if ((_data.Flags & 1) == 1)
                _respawn = new AutoRespawn(this, _data.Time);
        }
        private void WO_MOB_OnDestroy()
        {
            _view.FinishedInstantiation -= View_FinishedInstantiation;
            _respawn?.Destroy(); 
            _respawn = null;
            _threat.Destroy();
            _threat = null;
            _target = null;
            _tEntry = null;
        }
        private void View_FinishedInstantiation(PNetR.Player obj)
        {
            _stats.SendStats(obj);
        }
        #endregion
    }
}