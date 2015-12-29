using Ghost.Server.Utilities.Abstracts;
using System;

namespace Ghost.Server.Core.Events
{
    public class AutoRespawn : TimedEvent<WorldObject>
    {
        public AutoRespawn(WorldObject target, TimeSpan time)
            : base(target, time, false)
        { }
        public override void OnFire()
        {
            _data.Spawn();
        }
    }
}