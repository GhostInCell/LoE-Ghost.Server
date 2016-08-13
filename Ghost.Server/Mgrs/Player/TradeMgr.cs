using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Events;
using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Players;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNet;
using PNetR;
using System;
using System.Collections.Generic;

namespace Ghost.Server.Mgrs.Player
{
    [NetComponent(9)]
    public class TradeMgr : ObjectComponent
    {
        private class TradingStack : INetSerializable
        {
            public int Amount;
            public InventoryItem Item;

            public bool IsEmpty
            {
                get
                {
                    return Item.IsEmpty || Amount <= 0;
                }
            }

            public int AllocSize
            {
                get
                {
                    return 33;
                }
            }

            public TradingStack()
            {
                Item = new InventoryItem();
            }

            public void OnSerialize(NetMessage message)
            {
                Item.OnSerialize(message);
                message.Write(Amount);
            }

            public void OnDeserialize(NetMessage message)
            {
                Item.OnDeserialize(message);
                Amount = message.ReadInt32();
            }

        }

        private class TradingState : INetSerializable
        {
            public MapPlayer ONE;
            public MapPlayer TWO;
            public bool ONE_Ready;
            public bool TWO_Ready;
            public bool InProgress;
            public Dictionary<int, TradingStack> ONE_Offer;
            public Dictionary<int, TradingStack> TWO_Offer;

            private MapPlayer m_target;

            public int AllocSize
            {
                get
                {
                    return 12 + ONE_Offer.Count * 32 + TWO_Offer.Count * 32;
                }
            }

            public void Send(MapPlayer player)
            {
                m_target = player;
                player.View.Rpc(9, 12, RpcMode.OwnerOrdered, this);
            }

            public void SetReadyState(MapPlayer player, bool state)
            {
                if (player == ONE)
                    ONE_Ready = state;
                else
                    TWO_Ready = state;
            }

            public void OnSerialize(NetMessage message)
            {
                if (m_target == ONE)
                {
                    message.Write(ONE_Offer.Count);
                    foreach (var item in ONE_Offer.Values)
                        item.OnSerialize(message);
                    message.Write(TWO_Offer.Count);
                    foreach (var item in TWO_Offer.Values)
                        item.OnSerialize(message);
                    message.Write(InProgress);
                    message.Write(ONE_Ready);
                    message.Write(TWO_Ready);
                }
                else
                {
                    message.Write(TWO_Offer.Count);
                    foreach (var item in TWO_Offer.Values)
                        item.OnSerialize(message);
                    message.Write(ONE_Offer.Count);
                    foreach (var item in ONE_Offer.Values)
                        item.OnSerialize(message);
                    message.Write(InProgress);
                    message.Write(TWO_Ready);
                    message.Write(ONE_Ready);
                }
                message.WritePadBits();
            }

            public void OnDeserialize(NetMessage message)
            {
                throw new NotImplementedException();
            }
        }

        private static readonly object _lock = new object();
        private bool _trading;
        private bool _requested;
        private NetworkView _view;
        private WO_Player _wPlayer;
        private MapPlayer _player;
        private MapPlayer _target;
        private TradingState m_state;
        private TradeRejector _regected;
        private Dictionary<int, TradingStack> m_offer;

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
            _player = parent.Player;
            m_offer = new Dictionary<int, TradingStack>();
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
            m_state = null;
            _target = null;
            _trading = false;
            _regected = null;
            _requested = false;
            lock (_lock) m_offer.Clear();
        }
        public void UpdateState()
        {
            if (_trading && _target.Trade._trading)
            {
                if (m_state.ONE_Ready && m_state.TWO_Ready)
                {
                    lock (_lock)
                    {
                        ProcessTrade(_target.Trade.m_offer);
                        _target.Trade.ProcessTrade(m_offer);
                    }
                    CloseBoth();
                }
                else
                {
                    m_state.Send(_player);
                    m_state.Send(_target);
                }
            }
        }

        private void OfferUpdated()
        {
            lock (_lock)
            {
                foreach (var item in m_offer)
                {
                    if (!_player.Items.HasItems(item.Key, item.Value.Amount))
                    {
                        CloseBoth();
                        return;
                    }
                }
            }
            m_state.ONE_Ready = m_state.TWO_Ready = false;
            UpdateState();
        }

        private void ProcessTrade(Dictionary<int, TradingStack> _toffer)
        {
            lock (_lock)
            {
                foreach (var item in m_offer)
                    _player.Items.AddItems(item.Key, item.Value.Amount);
                foreach (var item in _toffer)
                    _player.Items.RemoveItems(item.Key, item.Value.Amount);
            }
        }
        #region RPC Handlers
        [Rpc(1, false)]
        private void Request(NetMessageInfo info)
        {
            MapPlayer sender = _player.Server[info.Sender.Id];
            if (_trading)
            {
                sender.View?.FailedTrade();
                return;
            }
            else if (sender.Trade._requested && sender.Trade._target == _player)
            {
                _target = sender;
                m_state = new TradingState();
                m_state.InProgress = true;
                m_state.ONE = _player;
                m_state.TWO = sender;
                m_state.ONE_Offer = m_offer;
                m_state.TWO_Offer = sender.Trade.m_offer;
                sender.Trade.m_state = m_state;
                sender.Trade._regected.Destroy();
                sender.Trade._trading = _trading = true;
                UpdateState();
                return;
            }
            else if (!_requested)
            {
                _target = sender;
                _requested = true;
                _view.RequestTrade(info.Sender.Id);
                _regected = new TradeRejector(_player, _target);
            }
        }

        [OwnerOnly]
        [Rpc(4, false)]
        private void Cancle()
        {
            if (_trading) CloseBoth();
        }

        [Rpc(6, false)]
        private void Offer(TradingStack slot)
        {
            if (_trading)
            {
                if (slot.IsEmpty)
                    _player.SystemMsg($"Inventory slot is empty!");
                else
                {
                    if (_target.Items.HasItems(slot.Item.Id, slot.Amount))
                    {
                        lock (_lock)
                        {
                            TradingStack current;
                            if (m_offer.TryGetValue(slot.Item.Id, out current))
                                current.Amount += slot.Amount;
                            else
                                m_offer[slot.Item.Id] = slot;
                        }
                    }
                    else
                        _player.SystemMsg($"You hasn't {slot.Amount} of {slot.Item.Id}");
                }
                _target.Trade.OfferUpdated();
            }
        }

        [OwnerOnly]
        [Rpc(8, false)]
        private void Ready()
        {
            if (_trading)
            {
                m_state.SetReadyState(_player, true);
                UpdateState();
            }
        }

        [OwnerOnly]
        [Rpc(9, false)]
        private void UnReady()
        {
            if (_trading)
            {
                m_state.SetReadyState(_player, false);
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
            m_offer = null;
            _target = null;
            _wPlayer = null;
            _player = null;
            _regected = null;
        }
        private void TradeMgr_OnDespawn()
        {
            if (_trading) { _target.Trade.Close(); ResetState(); }
        }
        #endregion
    }
}