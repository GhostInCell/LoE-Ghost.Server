using Ghost.Server.Utilities.Abstracts;
using System;

namespace Ghost.Server.Core.Events
{
    public class AutoDestroy : TimedEvent<WorldObject>
    {
        public AutoDestroy(WorldObject target, TimeSpan time)
            : base(target, time, false)
        { }
        public override void OnFire()
        {
            _data.Destroy();
        }
    }
}