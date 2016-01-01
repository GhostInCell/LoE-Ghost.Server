using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Events;
using Ghost.Server.Core.Players;
using PNet;
using PNetR;
using System.Collections.Generic;
using System.Linq;

namespace Ghost.Server.Mgrs.Player
{
    [NetComponent(5)]
    public class SkillsMgr
    {
        private MapPlayer _player;
        private TargetEntry _entry;
        private Dictionary<int, CooldownReset> _cooldown;
        public SkillsMgr(MapPlayer player)
        {
            _player = player;
            _entry = new TargetEntry();
            _cooldown = new Dictionary<int, CooldownReset>();
        }
        public void Destroy()
        {
            var toDestroy = _cooldown.ToArray();
            for (int i = 0; i < toDestroy.Length; i++)
                toDestroy[i].Value.Destroy();
            _cooldown.Clear();
            _entry = null;
            _player = null;
            toDestroy = null;
            _cooldown = null;
        }
        public void Initialize()
        {
            _player.Object.OnSpawn += SkillsMgr_OnSpawn;
            _player.View.SubscribeMarkedRpcsOnComponent(this);
        }
        public void RemoveCooldown(int skillID)
        {
            _cooldown.Remove(skillID);
        }
        public void AddCooldown(int skillID, float seconds)
        {
            _cooldown.Add(skillID, new CooldownReset(_player, skillID, seconds));
        }
        #region RPC Handlers
        [Rpc(61, false)]//Perform Skill
        private void RPC_061(NetMessage arg1, NetMessageInfo arg2)
        {
            _entry.OnDeserialize(arg1);
            if (!_player.Char.Data.Skills.ContainsKey(_entry.SkillID) || _cooldown.ContainsKey(_entry.SkillID))
                return;
            if (SpellsMgr.CanCast(_player, _entry))
                SpellsMgr.PerformSkill(_player, _entry);
        }
        [Rpc(62, false)]//Cancel Skill
        private void RPC_062(NetMessage arg1, NetMessageInfo arg2)
        {

        }
        #endregion
        #region Events Handlers
        private void SkillsMgr_OnSpawn()
        {
            _player.View.SubscribeMarkedRpcsOnComponent(this);
        }
        #endregion
    }
}