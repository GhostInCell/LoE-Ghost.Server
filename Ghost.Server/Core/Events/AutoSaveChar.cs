using Ghost.Server.Core.Players;
using Ghost.Server.Mgrs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using System;

namespace Ghost.Server.Core.Events
{
    public class AutoSaveChar : TimedEvent<MapPlayer>
    {
        private static readonly TimeSpan time;
        static AutoSaveChar()
        {
            time = TimeSpan.FromSeconds(Configs.Get<int>(Configs.Game_SaveChar));
        }
        public AutoSaveChar(MapPlayer target)
            : base(target, time, true)
        { }
        public override void OnFire()
        {
            _data.Data.Position = _data.Object.Position;
            _data.Data.Rotation = _data.Object.Rotation;
            CharsMgr.SaveCharacter(_data.Char);
        }
    }
}