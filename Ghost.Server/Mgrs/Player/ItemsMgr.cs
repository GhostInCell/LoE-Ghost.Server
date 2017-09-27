﻿using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Players;
using Ghost.Server.Core.Structs;
using Ghost.Server.Scripts;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNet;
using PNetR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ghost.Server.Mgrs.Player
{
    [NetComponent(7)]
    public class ItemsMgr : ObjectComponent
    {
        public struct WornSlot : INetSerializable
        {
            public byte Value;

            public int Index
            {
                get
                {
                    return Value - 1;
                }
            }

            public int AllocSize
            {
                get
                {
                    return 1;
                }
            }

            public bool IsWearable
            {
                get
                {
                    return Value > 0 && Value <= 32;
                }
            }

            public WearablePosition Position
            {
                get
                {
                    return Value.ToWearablePosition();
                }
            }

            public void OnSerialize(NetMessage message)
            {
                message.Write(Value);
            }

            public void OnDeserialize(NetMessage message)
            {
                Value = message.ReadByte();
            }

            public override string ToString()
            {
                return Position.ToString();
            }
        }

        public static readonly int ColorItemPrice = 10;

        private CharData m_data;
        private NetworkView m_view;
        private WO_Player _wPlayer;
        private MapPlayer _mPlayer;
        private PNetR.Player _player;
        private HashSet<int> m_itemsHash;
        private WearablePosition m_wearSlotsUsed;
        private Dictionary<int, InventoryItem> m_wears;
        private Dictionary<int, InventorySlot> m_items;

        public ItemsMgr(WO_Player parent)
            : base(parent)
        {
            _wPlayer = parent;
            _mPlayer = _wPlayer.Player;
            _player = _mPlayer.Player;
            m_data = _mPlayer.Data;
            m_items = m_data.Items;
            m_wears = m_data.Wears;
            m_itemsHash = new HashSet<int>();
            foreach (var item in m_items)
            {
                var value = item.Value;
                if (!m_itemsHash.Contains(value.Item.Id))
                    m_itemsHash.Add(value.Item.Id);
            }
            m_wearSlotsUsed = WearablePosition.None;
            foreach (var item in m_wears)
            {
                var value = item.Value;
                if (DataMgr.Select(value.Id, out DB_Item dbitem))
                    m_wearSlotsUsed |= dbitem.Slot;
            }
            parent.OnSpawn += ItemsMgr_OnSpawn;
            parent.OnDestroy += ItemsMgr_OnDestroy;
        }

        public bool HasItems(int id)
        {
            return m_itemsHash.Contains(id);
        }

        public void RemoveAllItems()
        {
            m_items.Clear();
            m_itemsHash.Clear();
            m_view.SetInventory(m_data);
        }

        public void AddBits(int bits)
        {
            m_data.Bits += bits;
            m_view.SetBits(m_data.Bits);
        }

        public void RemoveItems(int id)
        {
            if (!m_itemsHash.Contains(id)) return;
            m_itemsHash.Remove(id);
            for (int index = 0, count = m_items.Count; index < (m_data.InventorySlots - 1) && count > 0; index++)
            {
                if (m_items.TryGetValue(index, out var slot))
                {
                    count--;
                    if (slot.Item.Id == id)
                    {
                        slot.Amount = 0;
                        m_items.Remove(index);
                        m_view.UpdateSlot(slot, index);
                    }
                }
            }
        }

        public int GetItemsCount(int id)
        {
            return m_items.Sum(x => x.Value.Item.Id == id ? x.Value.Amount : 0);
        }

        public int AddItems(int id, int amount)
        {
            if (DataMgr.Select(id, out DB_Item item))
                return AddItem(item, amount);
            return amount;
        }

        public bool HasItems(int id, int amount)
        {
            return m_itemsHash.Contains(id) && GetItemsCount(id) >= amount;
        }

        public int RemoveItems(int id, int amount)
        {
            if (!m_itemsHash.Contains(id)) return amount;
            var itemAmount = GetItemsCount(id);
            if (itemAmount <= amount)
                RemoveItems(id);
            else
            {
                for (int index = 0, count = m_items.Count; index < (m_data.InventorySlots - 1) && count > 0 && amount > 0; index++)
                {
                    if (m_items.TryGetValue(index, out var slot))
                    {
                        count--;
                        if (slot.Item.Id == id)
                        {
                            var toRemove = Math.Min(slot.Amount, amount);
                            amount -= toRemove;
                            slot.Amount -= toRemove;
                            m_view.UpdateSlot(slot, index);
                            if (slot.Amount == 0)
                                m_items.Remove(index);
                        }
                    }
                }
                return amount;
            }
            return 0;
        }

        public bool HasInSlot(int index, int id, int amount)
            => m_items.TryGetValue(index, out var slot) && slot.Item.Id == id && slot.Amount >= amount;

        public int RemoveFromSlot(int index, int amount)
        {
            if (m_items.TryGetValue(index, out var slot))
            {
                var toRemove = Math.Min(slot.Amount, amount);
                amount -= toRemove;
                slot.Amount -= toRemove;
                m_view.UpdateSlot(slot, index);
                if (slot.Amount == 0)
                    m_items.Remove(index);
            }
            return amount;
        }

        private int GetFreeSlot()
        {
            for (int index = 0; index < (m_data.InventorySlots - 1); index++)
            {
                if (!m_items.TryGetValue(index, out var slot) || slot.IsEmpty)
                    return index;
            }
            return -1;
        }

        private int AddItem(DB_Item item, int amount)
        {
            if (m_itemsHash.Contains(item.ID) && (item.Flags & ItemFlags.Stackable) > 0)
            {
                for (int index = 0, count = m_items.Count; index < (m_data.InventorySlots - 1) && count > 0 && amount > 0; index++)
                {
                    if (m_items.TryGetValue(index, out var slot))
                    {
                        count--;
                        if (slot.Item.Id == item.ID && slot.Amount < item.Stack)
                            amount = AddSlot(index, item, amount);
                    }
                }
                if (amount > 0)
                    return AddNewSlots(item, amount);
            }
            else
                return AddNewSlots(item, amount);
            return 0;
        }

        private int AddNewSlots(DB_Item item, int amount)
        {
            var index = GetFreeSlot();
            if (index == -1)
                return -amount;
            if (!m_itemsHash.Contains(item.ID))
                m_itemsHash.Add(item.ID);
            while (index != -1 && (amount = SetSlot(index, item, amount)) > 0)
                index = GetFreeSlot();
            return amount;
        }

        private int AddItem(int index, DB_Item item, int amount)
        {
            if (!m_items.ContainsKey(index))
                return amount;
            if (!m_itemsHash.Contains(item.ID))
                m_itemsHash.Add(item.ID);
            return SetSlot(index, item, amount);
        }

        private void SetSlot(int index, InventorySlot islot)
        {
            var slot = new InventorySlot() { Item = islot.Item, Amount = islot.Amount };
            m_items[index] = slot;
            m_view.UpdateSlot(slot, index);
        }

        private int SetSlot(int index, DB_Item item, int amount)
        {
            var slot = new InventorySlot(item.ID, 0);
            if ((item.Flags & ItemFlags.Stackable) == 0)
                slot.Amount = 1;
            else
                slot.Amount = amount < item.Stack ? amount : item.Stack;
            m_items[index] = slot;
            m_view.UpdateSlot(slot, index);
            return amount - slot.Amount;
        }

        private int AddSlot(int index, DB_Item item, int amount)
        {
            if ((item.Flags & ItemFlags.Stackable) == 0) return amount;
            var slot = m_items[index];
            var nAmount = amount + slot.Amount;
            slot.Amount = nAmount < item.Stack ? nAmount : item.Stack;
            m_view.UpdateSlot(slot, index);
            return amount - slot.Amount;
        }

        private void SetSlot(int index, InventoryItem item, int amount)
        {
            if (!m_items.TryGetValue(index, out var slot))
                m_items[index] = slot = new InventorySlot();
            slot.Item = item;
            slot.Amount = amount;
            m_view.UpdateSlot(slot, index);
        }

        private void RemoveSlot(int index, InventorySlot slot)
        {
            if (slot != null || m_items.TryGetValue(index, out slot))
            {
                slot.Amount = 0;
                m_view.UpdateSlot(slot, index);
                m_items.Remove(index);
            }
        }

        #region RPC Handlers
        [Rpc(4, false)]//WornItems
        private void RPC_004(NetMessage message, NetMessageInfo info)
        {
            m_view.WornItems(info.Sender, m_data);
        }

        [OwnerOnly]
        [Rpc(6, false)]
        private void AddItem(int itemId, int amount)
        {
            if (_mPlayer.User.Access >= AccessLevel.TeamMember)
            {
                if (itemId == -1)
                {
                    AddBits(amount);
                    _player.SystemMsg($"Added {amount} bits");
                    return;
                }
                if (DataMgr.Select(itemId, out DB_Item item))
                {
                    var count = AddItem(item, amount);
                    if (count != 0)
                        _player.SystemMsg($"Inventory full, added {(count == -1 ? 0 : amount - count)}/{amount} items");
                    else
                        _player.SystemMsg($"Added item {item.Name ?? item.ID.ToString()} amount {amount}");
                }
                else
                    _player.SystemMsg($"Item {itemId} not found");
            }
            else
                _player.SystemMsg($"You haven't permission to adding items");
        }

        [Rpc(7, false)]
        private void DeleteItem(byte islot, int amount)
        {
            if (m_items.TryGetValue(islot, out var slot))
            {
                int ramount = RemoveItems(slot.Item.Id, amount);
                if (ramount == 0)
                    _player.SystemMsg($"Removed {amount} items {slot.Item.Id} from {islot}");
                else

                    _player.SystemMsg($"Error while removing items {slot.Item.Id} from {islot} removed {amount - ramount}/{amount}");
            }
            else _player.SystemMsg($"Inventory slot {islot} is empty");
        }

        [OwnerOnly]
        [Rpc(8, false)]
        private void WearItem(WornSlot wslot, byte islot)
        {
            if (m_items.TryGetValue(islot, out var slot))
            {
                if (DataMgr.Select(slot.Item.Id, out DB_Item item))
                {
                    var wPosition = wslot.Position;
                    if ((item.Slot & wPosition) > 0)
                    {
                        var flags = item.Flags;
                        var index = wslot.Index;
                        RemoveSlot(islot, slot);
                        if (m_wears.TryGetValue(index, out var witem))
                        {
                            item = DataMgr.SelectItem(witem.Id);
                            SetSlot(islot, witem, 1);
                            flags |= item.Flags;
                        }
                        m_wearSlotsUsed |= wPosition;
                        m_wears[index] = slot.Item;
                        m_view.WearItem(slot.Item, wslot.Value, m_wearSlotsUsed);
                        if ((flags & ItemFlags.Stats) > 0)
                            _mPlayer.Stats.UpdateStats();
                    }
                    else
                        _player.SystemMsg($"You can't wear {item.Name} in {wPosition}");
                }
                else
                    _player.SystemMsg($"Item {slot.Item.Id} not found");
            }
            else _player.SystemMsg($"Inventory slot {islot} is empty");
        }

        [OwnerOnly]
        [Rpc(9, false)]
        private void UnwearItem(WornSlot wslot, byte islot)
        {
            var index = wslot.Index;
            if (m_wears.TryGetValue(index, out var witem))
            {
                if (DataMgr.Select(witem.Id, out DB_Item item))
                {
                    if (m_items.TryGetValue(islot, out var slot))
                    {
                        var itemSlot = GetFreeSlot();
                        if (itemSlot != -1)
                        {
                            SetSlot(itemSlot, witem, 1);
                            m_wears.Remove(index);
                            m_wearSlotsUsed &= ~wslot.Position;
                            m_view.UnwearItem(wslot.Value, m_wearSlotsUsed);
                            if ((item.Flags & ItemFlags.Stats) > 0)
                                _mPlayer.Stats.UpdateStats();
                        }
                        else
                        {
                            _player.SystemMsg($"You inventory is full");
                            m_view.WearItem(witem, wslot.Value, m_wearSlotsUsed);
                        }
                    }
                    else SetSlot(islot, witem, 1);
                }
                else
                {
                    _player.SystemMsg($"Item {witem.Id} not found");
                    m_view.WearItem(witem, wslot.Value, m_wearSlotsUsed);
                }
            }
            else
                _player.SystemMsg($"Wear slot {wslot.Position} is empty");
        }

        [OwnerOnly]
        [Rpc(12, false)]//Use item
        private void UseItem(byte islot)
        {
            if (m_items.TryGetValue(islot, out var slot))
            {
                if (DataMgr.Select(slot.Item.Id, out DB_Item item))
                {
                    if ((item.Flags & ItemFlags.Usable) > 0)
                        ItemsScript.Use(item.ID, _mPlayer);
                    else _player.SystemMsg($"You can't use item {item.Name ?? item.ID.ToString()}");
                }
                else _player.SystemMsg($"Item {slot.Item.Id} not found");
            }
            else _player.SystemMsg($"Inventory slot {islot} is empty");
        }

        [OwnerOnly]
        [Rpc(14, false)]
        private void ColorItem(byte islot, byte colors, uint color01, uint color02)
        {
            if (m_data.Bits >= ColorItemPrice)
            {
                if (m_items.TryGetValue(islot, out var slot))
                {
                    AddBits(-ColorItemPrice);
                    if ((colors & 0x1) > 0) slot.Item.Color01 = color01;
                    if ((colors & 0x2) > 0) slot.Item.Color02 = color02;
                    m_view.UpdateSlot(slot, islot);
                }
                else _player.SystemMsg($"Inventory slot {islot} is empty");
            }
            else
                _player.SystemMsg($"You have not enough bits {ColorItemPrice - m_data.Bits}");
        }

        [OwnerOnly]
        [Rpc(20, false)]
        private void MoveSlotToSlot(int first, int second)
        {
            if (m_items.TryGetValue(first, out var fSlot))
            {
                if (m_items.TryGetValue(second, out var sSlot))
                {
                    SetSlot(first, sSlot);
                    SetSlot(second, fSlot);
                }
                else
                {
                    SetSlot(second, fSlot);
                    RemoveSlot(first, fSlot);
                }
            }
            else _player.SystemMsg($"Inventory slot {first} is empty");
        }
        #endregion
        #region Events Handlers
        private void ItemsMgr_OnSpawn()
        {
            m_view = _wPlayer.View;
            m_view.SubscribeMarkedRpcsOnComponent(this);
        }

        private void ItemsMgr_OnDestroy()
        {
            m_view = null;
            m_data = null;
            m_wears = null;
            m_items = null;
            _player = null;
            _mPlayer = null;
            _wPlayer = null;
            m_itemsHash = null;
        }
        #endregion
    }
}