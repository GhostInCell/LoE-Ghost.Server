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
        private const int runMod = 18;
        private const int interval = 200;
        private int _update;
        private string _status;
        private MapPlayer _mPlayer;
        public string Status
        {
            get
            {
                return _status;
            }
        }
        public override int MeleeSkill
        {
            get
            {
                return 0;
            }
        }
        public override float AttackRate
        {
            get
            {
                return 0f;
            }
        }
        public override float MeleeDamage
        {
            get
            {
                return 0f;
            }
        }
        public PlayerStatsMgr(WO_Player parent)
            : base(parent)
        {
            _mPlayer = parent.Player;
            if (_stats.Count == 0) CreateBase();
            parent.OnDestroy += PlayerStatsMgr_OnDestroy;
            UpdateStats();
        }
        public override void UpdateStats()
        {
            foreach (var stat in _stats)
                stat.Value.CleanItems();
            foreach (var item in _mPlayer.Char.Data.Wears)
            {
                var entry = DataMgr.SelectItem(item.Value.Id);
                if ((entry.Flags & ItemFlags.Stats) > 0)
                    foreach (var stat in entry.Stats)
                        if (_stats.ContainsKey(stat.Item1))
                            _stats[stat.Item1].AddItemModifer(stat.Item2);
                        else
                            ServerLogger.LogWarn($"Player stat {stat.Item1} not found");
            }
            if (_view != null) SendStats();
        }
        public override void Update(TimeSpan time)
        {
            if (_creature.IsDead || (_update -= time.Milliseconds) > 0) return;
            _update = interval;
            var hp = _stats[Stats.Health];
            var ep = _stats[Stats.Energy];
            if (_creature.Movement.IsRunning && _mPlayer.User.Access < AccessLevel.TeamMember)
            {
                ep.DecreaseCurrent(runMod * (interval / 1000f));
                if (ep.Current == 0) _creature.Movement.Lock();
            }
            if (hp.Max != hp.Current)
            {
                hp.IncreaseCurrent(_stats[Stats.HealthRegen].Max * (interval / 1000f));
                _view.Rpc(4, 50, RpcMode.AllUnordered, (byte)Stats.Health, hp.Current);
            }
            if (ep.Max != ep.Current)
            {
                ep.IncreaseCurrent(_stats[Stats.EnergyRegen].Max * (interval / 1000f));
                _view.Rpc(4, 50, RpcMode.AllUnordered, (byte)Stats.Energy, ep.Current);
            }
            _status = $"HP {hp.Current}/{hp.Max}; EP {ep.Current}/{ep.Max}";
        }
        public void AddExpAll(uint exp, uint bonusExp = 0)
        {
            uint[] talents = _mPlayer.Data.Talents.Keys.ToArray();
            for (int i = 0; i < talents.Length; i++)
                AddExp((TalentMarkId)talents[i], exp, bonusExp);
            talents = null;
        }
        public void AddExp(TalentMarkId talant, uint exp, uint bonusExp = 0)
        {
            var talantState = _mPlayer.Data.Talents[(uint)talant];
            if (talantState.Item2 >= CharsMgr.MaxLevel)
                return;
            if (CalculateTalentLevel(ref talantState, exp + bonusExp))
            {
                UpdateBase();
                _mPlayer.Player.Rpc(4, _mPlayer.Data.SerTalents);
                _mPlayer.Player.Rpc(3, (uint)talant, talantState.Item1, (uint)talantState.Item2);
                _view.Rpc(4, 53, RpcMode.AllUnordered, (object)_mPlayer.Char.Level);
            }
            else
                _mPlayer.Player.Rpc(2, (uint)talant, exp, bonusExp);
            _mPlayer.Data.Talents[(uint)talant] = talantState;
        }
        private short GetLevel()
        {
            short ret = _mPlayer.Data.Talents.Values.Max(x => x.Item2);
            return ret > CharsMgr.MaxLevel ? CharsMgr.MaxLevel : ret;
        }
        private void CreateBase()
        {
            _level = (short)(_mPlayer.Char.Level == 0 ? 1 : _mPlayer.Char.Level);
            _stats[Stats.Dodge] = new StatValue(0);
            _stats[Stats.Armor] = new StatValue(0);
            _stats[Stats.MagicResist] = new StatValue(0);
            _stats[Stats.Health] = new StatValue(250 + _level * 100);
            _stats[Stats.Attack] = new StatValue(1 + (_level - 1) * 17);
            switch (_mPlayer.Char.Pony.Race)
            {
                case 1:
                    _stats[Stats.Speed] = new StatValue(350);
                    _stats[Stats.Energy] = new StatValue(125);
                    _stats[Stats.EnergyRegen] = new StatValue(10);
                    _stats[Stats.HealthRegen] = new StatValue(20 + _level * 10);
                    break;
                case 2:
                    _stats[Stats.Speed] = new StatValue(307);
                    _stats[Stats.Energy] = new StatValue(100);
                    _stats[Stats.EnergyRegen] = new StatValue(11);
                    _stats[Stats.HealthRegen] = new StatValue(28 + (_level - 1) * 9.5f);
                    break;
                case 3:
                    _stats[Stats.Speed] = new StatValue(310);
                    _stats[Stats.Energy] = new StatValue(100);
                    _stats[Stats.EnergyRegen] = new StatValue(11);
                    _stats[Stats.HealthRegen] = new StatValue(28 + (_level - 1) * 9.5f);
                    break;
            }
        }
        private void UpdateBase()
        {
            _mPlayer.Char.Level = _level = GetLevel();
            _stats[Stats.Health].SetBase(250 + _level * 100);
            _stats[Stats.Attack].SetBase(1 + (_level - 1) * 17);
            switch (_mPlayer.Char.Pony.Race)
            {
                case 1:
                    _stats[Stats.HealthRegen].SetBase(20 + _level * 10);
                    break;
                case 2:
                case 3:
                    _stats[Stats.HealthRegen].SetBase(28 + (_level - 1) * 9.5f);
                    break;
            }
            SendStats();
        }
        private bool CalculateTalentLevel(ref Tuple<uint, short, short> talent, uint exp)
        {
            if (talent.Item2 >= CharsMgr.MaxLevel)
                return false;
            var cExp = talent.Item1 + exp;
            var level = (short)(talent.Item2 <= 0 ? 1 : talent.Item2);
            while (level < CharsMgr.MaxLevel)
            {
                var nExp = CharsMgr.GetExpForLevel(level);
                if (cExp < nExp) break;
                cExp -= nExp;
                level++;
            }
            if (level != talent.Item2)
            {
                talent = new Tuple<uint, short, short>(cExp, level, (short)(talent.Item3 + ((level - talent.Item2) * CharsMgr.TalentPointsPerLevel)));
                return true;
            }
            else
            {
                talent = new Tuple<uint, short, short>(cExp, level, talent.Item3);
                return false;
            }
        }
        #region Events Handlers
        private void PlayerStatsMgr_OnDestroy()
        {
            _mPlayer.Char.Level = GetLevel();
            _status = null;
            _mPlayer = null;
        }
        #endregion
    }
}