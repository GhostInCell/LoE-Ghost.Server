using Ghost.Server.Core.Events;
using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Players;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNet;
using PNetR;
using System.Collections.Generic;

namespace Ghost.Server.Mgrs.Player
{
    [NetComponent(9)]
    public class TradeMgr : ObjectComponent
    {
        private static readonly object _lock = new object();
        private bool _ready;
        private bool _trading;
        private bool _requested;
        private NetworkView _view;
        private WO_Player _wPlayer;
        private MapPlayer _mPlayer;
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
        public TradeMgr(WO_Player parent)
            : base(parent)
        {
            _wPlayer = parent;
            _mPlayer = _wPlayer.Player;
            _offer = new Dictionary<int, int>();
            _offerItems = new SER_Trade(_offer);
            parent.OnSpawn += TradeMgr_OnSpawn;
            parent.OnDestroy += TradeMgr_OnDestroy;
            parent.OnDespawn += TradeMgr_OnDespawn;
        }
        public void Close()
        {
            if (_trading)
            {
                _view.CloseTrade();
                ResetState();
            }
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
                    _view.SendTradeState(_offerItems, _target.Trade._offerItems, _ready, _target.Trade._ready);
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
                    if (!_mPlayer.Items.HasItems(item.Key, item.Value))
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
                    _mPlayer.Items.RemoveItems(item.Key, item.Value);
                foreach (var item in _toffer)
                    _mPlayer.Items.AddItems(item.Key, item.Value);
            }
        }
        #region RPC Handlers
        [Rpc(1)]//Trade Request
        private void RPC_001(NetMessage arg1, NetMessageInfo arg2)
        {
            MapPlayer sender = _mPlayer.Server[arg2.Sender.Id];
            if (_trading)
            {
                sender.View?.FailedTrade();
                return;
            }
            else if (sender.Trade._requested && sender.Trade._target == _mPlayer)
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
                _view.RequestTrade(arg2.Sender.Id);
                _regected = new TradeRejector(_mPlayer, _target);
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
            _view = _wPlayer.View;
            _view.SubscribeMarkedRpcsOnComponent(this);
        }
        private void TradeMgr_OnDestroy()
        {
            _regected?.Destroy();
            if (_trading && _target.Trade._trading)
                _target.Trade.Close();
            _view = null;
            _offer = null;
            _target = null;
            _wPlayer = null;
            _mPlayer = null;
            _regected = null;
            _offerItems = null;
        }
        private void TradeMgr_OnDespawn()
        {
            if (_trading) { _target.Trade.Close(); ResetState(); }
        }
        #endregion
    }
}