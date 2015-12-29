using Ghost.Server.Core.Players;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using System;

namespace Ghost.Server.Core.Events
{
    public class TradeRejector : TimedEvent<MapPlayer, MapPlayer>
    {
        private static readonly TimeSpan time;
        static TradeRejector()
        {
            time = TimeSpan.FromSeconds(5);
        }
        public TradeRejector(MapPlayer target, MapPlayer initiator)
            : base(target, initiator, time, false)
        { }
        public override void OnFire()
        {
            _data01.Trade.ResetState();
            _data02.View?.FailedTrade();
        }
    }
}