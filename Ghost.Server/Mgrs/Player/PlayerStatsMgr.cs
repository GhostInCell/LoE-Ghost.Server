using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Players;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNet;
using System;
using System.Linq;

namespace Ghost.Server.Mgrs.Player
{
    public class PlayerStatsMgr : StatsMgr
    {
        private const int runMod = -18;
        private const int interval = 200;
        private int _update;
        private string _status;
        private MapPlayer _player;
        public readonly SER_Stats SerStats;
        public string Status
        {
            get
            {
                return _status;
            }
        }
        public PlayerStatsMgr(WO_Player obj) 
            : base(obj)
        {
            _player = obj.Player;
            SerStats = new SER_Stats(_stats);
            if (_stats.Count == 0) CreateBase();
            UpdateStats();
        }
        public override void UpdateStats()
        {
            foreach (var stat in _stats)
                stat.Value.Clean();
            foreach (var item in _player.Char.Data.Wears)
            {
                var entry = DataMgr.SelectItem(item.Value);
                if ((entry.Flags & ItemFlags.Stats) > 0)
                    foreach (var stat in entry.Stats)
                        if (_stats.ContainsKey(stat.Item1))
                            _stats[stat.Item1].UpdateItem(stat.Item2);
                        else
                            ServerLogger.LogWarn($"Player stat {stat.Item1} not found");
            }
        }
        public override void Destroy()
        {
            _player.Char.Level = GetLevel();
            _stats.Clear();
            _stats = null;
            _player = null;
            _status = null;
            _creature = null;
        }
        public void UpdateItemsStats()
        {
            UpdateStats();
            _creature.View.UpdateStats(SerStats);
        }
        public override void Update(TimeSpan time)
        {
            if ((_update -= time.Milliseconds) > 0) return;
            _update = interval;
            var hp = _stats[Stats.Health];
            var ep = _stats[Stats.Energy];
            if (_creature.Movement.IsRunning)
            {
                ep.UpdateCurrent(runMod * (interval / 1000f));
                if (ep.Current == 0) _creature.Movement.Lock();
            }
            if (hp.Max != hp.Current)
            {
                hp.UpdateCurrent(_stats[Stats.HealthRegen].Max * (interval / 1000f));
                _creature.View.Rpc(4, 51, RpcMode.AllOrdered, (byte)Stats.Health, hp.Max);
                _creature.View.Rpc(4, 50, RpcMode.AllOrdered, (byte)Stats.Health, hp.Current);
            }
            if (ep.Max != ep.Current)
            {
                ep.UpdateCurrent(_stats[Stats.EnergyRegen].Max * (interval / 1000f));
                _creature.View.Rpc(4, 51, RpcMode.AllOrdered, (byte)Stats.Energy, ep.Max);
                _creature.View.Rpc(4, 50, RpcMode.AllOrdered, (byte)Stats.Energy, ep.Current);
            }
            _status = $"HP {hp.Current}/{hp.Max}; EP {ep.Current}/{ep.Max}";
        }
        public void ModCurren(Stats stat, float value)
        {
            if (!_stats.ContainsKey(stat)) return;
            _stats[stat].UpdateCurrent(value);
            if (stat == Stats.Health && Health == 0f)
            {
                _creature.View.Rpc(4, 51, RpcMode.AllOrdered, (byte)Stats.Health, _stats[stat].Max);
                _creature.View.Rpc(4, 50, RpcMode.AllOrdered, (byte)Stats.Health, 0f);
                _creature.Despawn();
            }
        }
        public void AddExpAll(uint exp, uint bonusExp = 0)
        {
            int[] talents = _player.Data.Talents.Keys.ToArray();
            for (int i = 0; i < talents.Length; i++)
                AddExp((Talent)talents[i], exp, bonusExp);
            talents = null;
        }
        public void AddExp(Talent talant, uint exp, uint bonusExp = 0)
        {
            var talantState = _player.Data.Talents[(int)talant];
            if (talantState.Item2 >= CharsMgr.MaxLevel) return;
            var cLevel = talantState.Item2;
            var cExp = talantState.Item1 + exp + bonusExp;
            var nExp = CalculateExpForLevel(talantState.Item2);
            var nLevel = cExp >= nExp ? CalculateNewLevel(talantState.Item2, nExp, ref cExp) : talantState.Item2;
            _player.Data.Talents[(int)talant] = new Tuple<uint, short>(cExp, nLevel);
            if (cLevel != nLevel)
            {
                UpdateBase();
                _player.Player.Rpc(4, _player.Data.SerTalents);
                _player.Player.Rpc(3, (int)talant, cExp, (int)nLevel);
                _creature.View.Rpc(4, 52, RpcMode.OwnerUnordered, SerStats);
                _creature.View.Rpc(4, 53, RpcMode.AllUnordered, _player.Char.Level);
            }
            else _player.Player.Rpc(2, (int)talant, exp, bonusExp);
        }
        public override void DoDamage(CreatureObject other, float damage, bool isMagic = false)
        {
            StatHelper hStat = _stats[Stats.Health];
            StatHelper pStat = isMagic ? _stats[Stats.MagicResist] : _stats[Stats.Armor];
            hStat.UpdateCurrent(-CalculateDamage(other.Stats.Level, damage, pStat.Max));
            if (hStat.Current == 0f)
            {
                _creature.View.Rpc(4, 51, RpcMode.AllOrdered, (byte)Stats.Health, hStat.Max);
                _creature.View.Rpc(4, 50, RpcMode.AllOrdered, (byte)Stats.Health, 0f);
                _creature.Despawn();
            } 
        }
        private short GetLevel()
        {
            short ret = (short)_player.Data.Talents.Values.Sum(x => x.Item2);
            return ret > CharsMgr.MaxLevel ? CharsMgr.MaxLevel : ret;
        }
        private void CreateBase()
        {
            _level = (short)(_player.Char.Level == 0 ? 1 : _player.Char.Level);
            _stats[Stats.Dodge] = new StatHelper(0);
            _stats[Stats.Armor] = new StatHelper(0);
            _stats[Stats.MagicResist] = new StatHelper(0);
            _stats[Stats.Health] = new StatHelper(250 + _level * 100);
            _stats[Stats.Attack] = new StatHelper(1 + (_level - 1) * 17);
            switch (_player.Char.Pony.Race)
            {
                case 1:
                    _stats[Stats.Speed] = new StatHelper(350);
                    _stats[Stats.Energy] = new StatHelper(125);
                    _stats[Stats.EnergyRegen] = new StatHelper(10);
                    _stats[Stats.HealthRegen] = new StatHelper(20 + _level * 10);
                    break;
                case 2:
                    _stats[Stats.Speed] = new StatHelper(307);
                    _stats[Stats.Energy] = new StatHelper(100);
                    _stats[Stats.EnergyRegen] = new StatHelper(11);
                    _stats[Stats.HealthRegen] = new StatHelper(28 + (_level - 1) * 9.5f);
                    break;
                case 3:
                    _stats[Stats.Speed] = new StatHelper(310);
                    _stats[Stats.Energy] = new StatHelper(100);
                    _stats[Stats.EnergyRegen] = new StatHelper(11);
                    _stats[Stats.HealthRegen] = new StatHelper(28 + (_level - 1) * 9.5f);
                    break;
            }
        }
        private void UpdateBase()
        {
            _player.Char.Level = _level = GetLevel();
            _stats[Stats.Health].SetBase(250 + _level * 100);
            _stats[Stats.Attack].SetBase(1 + (_level - 1) * 17);
            switch (_player.Char.Pony.Race)
            {
                case 1:
                    _stats[Stats.HealthRegen].SetBase(20 + _level * 10);
                    break;
                case 2:
                case 3:
                    _stats[Stats.HealthRegen].SetBase(28 + (_level - 1) * 9.5f);
                    break;
            }
        }
        private uint CalculateExpForLevel(short level)
        {
            if (level <= 0) level = 1;
            else if (level > CharsMgr.MaxLevel) level = CharsMgr.MaxLevel;
            return (uint)(level * 500 + (level - 1) * 500);
        }
        private short CalculateNewLevel(short cLevel, uint nExp, ref uint cExp)
        {
            cExp -= nExp; cLevel++;
            if (cLevel >= CharsMgr.MaxLevel) return CharsMgr.MaxLevel;
            while (cExp >= (nExp = CalculateExpForLevel(cLevel)) && cLevel < CharsMgr.MaxLevel)
            { cExp -= nExp; cLevel++; }
            return cLevel;
        }
    }
}