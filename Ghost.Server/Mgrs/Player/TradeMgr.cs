using Ghost.Server.Core.Events;
using Ghost.Server.Core.Players;
using Ghost.Server.Utilities;
using PNet;
using PNetR;
using System;
using System.Collections.Generic;

namespace Ghost.Server.Mgrs.Player
{
    [NetComponent(9)]
    public class TradeMgr
    {
        private static readonly object _lock = new object();
        private bool _ready;
        private bool _trading;
        private bool _requested;
        private MapPlayer _player;
        private MapPlayer _target;
        private SER_Trade _offerItems;
        private TradeRejector _regected;
        private Dictionary<int, int> _offer;
        public bool IsTrading
        {
            get { return _trading; }
        }
        public MapPlayer Target
        {
            get { return _target; }
        }
        public TradeMgr(MapPlayer player)
        {
            _player = player;
            _offer = new Dictionary<int, int>();
            _offerItems = new SER_Trade(_offer);
        }
        public void Close()
        {
            if (_trading)
            {
                _player.View.CloseTrade();
                ResetState();
            }
        }
        public void Destroy()
        {
            _regected?.Destroy();
            if (_trading && _target.Trade._trading)
                _target.Trade.Close();
            lock (_lock) _offer.Clear();
            _offer = null;
            _player = null;
            _target = null;
            _regected = null;
            _offerItems = null;
        }
        public void CloseBoth()
        {
            _target?.Trade.Close();
            Close();
        }
        public void ResetState()
        {
            _regected?.Destroy();
            _target = null;
            _ready = false;
            _trading = false;
            _regected = null;
            _requested = false;
            lock (_lock) _offer.Clear();
        }
        public void Initialize()
        {
            _player.Object.OnSpawn += TradeMgr_OnSpawn;
            _player.Object.OnDespawn += TradeMgr_OnDespawn;
            _player.View.SubscribeMarkedRpcsOnComponent(this);
        }
        public void UpdateState()
        {
            if (_trading && _target.Trade._trading)
            {
                if (_ready && _target.Trade._ready)
                {
                    _ready = false; _target.Trade._ready = false;
                    lock (_lock)
                    {
                        ProcessTrade(_target.Trade._offer);
                        _target.Trade.ProcessTrade(_offer);
                    }
                    CloseBoth();
                }
                else
                {
                    _player.View.SendTradeState(_offerItems, _target.Trade._offerItems, _ready, _target.Trade._ready);
                    _target.View.SendTradeState(_target.Trade._offerItems, _offerItems, _target.Trade._ready, _ready);
                }
            }
        }
        private void OfferUpdated()
        {
            lock (_lock)
            {
                foreach (var item in _offer)
                {
                    if (!_player.Items.HasItems(item.Key, item.Value))
                    {
                        CloseBoth();
                        return;
                    }
                }
            }
            _target.Trade._ready = _ready = false;
            UpdateState();
        }
        private void ProcessTrade(Dictionary<int, int> _toffer)
        {
            lock (_lock)
            {
                foreach (var item in _offer)
                    _player.Items.RemoveItems(item.Key, item.Value);
                foreach (var item in _toffer)
                    _player.Items.AddItems(item.Key, item.Value);
            }
        }
        #region RPC Handlers
        [Rpc(1)]//Trade Request
        private void RPC_001(NetMessage arg1, NetMessageInfo arg2)
        {
            MapPlayer sender = _player.Server[arg2.Sender.Id];
            if (_trading)
            {
                sender.View?.FailedTrade();
                return;
            }
            else if (sender.Trade._requested && sender.Trade._target == _player)
            {
                _target = sender;
                sender.Trade._regected.Destroy();
                sender.Trade._trading = _trading = true;
                UpdateState();
                return;
            }
            else if (!_requested)
            {
                _target = sender;
                _requested = true;
                _player.View.RequestTrade(arg2.Sender.Id);
                _regected = new TradeRejector(_player, _target);
            }
        }
        [Rpc(4)]//Trade Cancle
        private void RPC_004(NetMessage arg1, NetMessageInfo arg2)
        {
            if (_trading) CloseBoth();
        }
        [Rpc(6)]//Offer
        private void RPC_006(NetMessage arg1, NetMessageInfo arg2)
        {
            if (_trading && arg2.Sender.Id == _target.Player.Id)
            {
                lock (_lock)
                    _target.Trade._offerItems.OnDeserialize(arg1);
                _target.Trade.OfferUpdated();
            }
        }
        [Rpc(8)]//Trade Ready
        private void RPC_008(NetMessage arg1, NetMessageInfo arg2)
        {
            if (_trading)
            {
                _target.Trade._ready = true;
                UpdateState();
            }
        }
        [Rpc(9)]//Trade UnReady
        private void RPC_009(NetMessage arg1, NetMessageInfo arg2)
        {
            if (_trading)
            {
                _target.Trade._ready = false;
                UpdateState();
            }
        }
        #endregion
        #region Events Handlers
        private void TradeMgr_OnSpawn()
        {
            _player.View.SubscribeMarkedRpcsOnComponent(this);
        }
        private void TradeMgr_OnDespawn()
        {
            if (_trading) { _target.Trade.Close(); ResetState(); }
        }
        #endregion
    }
}