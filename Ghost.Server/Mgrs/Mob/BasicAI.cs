using Ghost.Server.Utilities.Abstracts;
using System;

namespace Ghost.Server.Mgrs.Mob
{
    public class BasicAI : ScriptedAI
    {
        public BasicAI(CreatureObject parent)
            : base(parent)
        { }
        public override void OnUpdate(TimeSpan time)
        {
            if (_threat.SelectTarget(out _target))
                DoMeleeAttackIfReady();
        }
    }
}