using Ghost.Server.Core.Classes;
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
        private static readonly Tuple<int, int> Empty = new Tuple<int, int>(-1, -1);
        private CharData _data;
        private NetworkView _view;
        private WO_Player _wPlayer;
        private MapPlayer _mPlayer;
        private PNetR.Player _player;
        private HashSet<int> _itemsHash;
        private Dictionary<byte, int> _wears;
        private Dictionary<byte, Tuple<int, int>> _items;
        public ItemsMgr(WO_Player parent)
            : base(parent)
        {
            _wPlayer = parent;
            _mPlayer = _wPlayer.Player;
            _player = _mPlayer.Player;
            _data = _mPlayer.Data;
            _items = _data.Items;
            _wears = _data.Wears;
            _itemsHash = new HashSet<int>();
            foreach (var item in _items)
                if (!_itemsHash.Contains(item.Value.Item1))
                    _itemsHash.Add(item.Value.Item1);
            parent.OnSpawn += ItemsMgr_OnSpawn;
            parent.OnDestroy += ItemsMgr_OnDestroy;
        }
        public bool HasItems(int id)
        {
            return _itemsHash.Contains(id);
        }
        public void RemoveAllItems()
        {
            _items.Clear();
            _itemsHash.Clear();
            _view.SetInventory(_data);
        }
        public void AddBits(int bits)
        {
            _data.Bits += bits;
            _view.SetBits(_data.Bits);
        }
        public void RemoveItems(int id)
        {
            if (!_itemsHash.Contains(id)) return;
            _itemsHash.Remove(id);
            Tuple<int, int> item;
            for (byte i = 0, cnt = (byte)_items.Count; i < _data.InvSlots && cnt > 0; i++)
                if (_items.TryGetValue(i, out item))
                {
                    cnt--;
                    if (item.Item1 == id)
                    {
                        _items.Remove(i);
                        _view.DeleteItem(i, item.Item2);
                    }
                }
        }
        public int GetItemsCount(int id)
        {
            return _items.Sum(x => x.Value.Item1 == id ? x.Value.Item2 : 0);
        }
        public void ClearSlot(byte islot)
        {
            var slot = GetSlot(islot);
            if (slot.Item1 != -1)
            {
                _items.Remove(islot);
                _view.DeleteItem(islot, slot.Item2);
            }
        }
        public int AddItems(int id, int amount)
        {
            DB_Item item;
            if (DataMgr.Select(id, out item))
                return AddItem(item, amount);
            return amount;
        }
        public bool HasItems(int id, int amount)
        {
            return _itemsHash.Contains(id) && GetItemsCount(id) >= amount;
        }
        public int RemoveItems(int id, int amount)
        {
            if (!_itemsHash.Contains(id)) return amount;
            var itemAmount = GetItemsCount(id);
            if (itemAmount <= amount)
                RemoveItems(id);
            else
            {
                Tuple<int, int> item;
                for (byte i = 0; i < _data.InvSlots; i++)
                    if (_items.TryGetValue(i, out item) && item.Item1 == id)
                        amount = RemoveSlot(i, id, amount);
                return amount;
            }
            return 0;
        }
        public bool HasInSlot(byte islot, int id, int amount)
        {
            Tuple<int, int> slot;
            return _items.TryGetValue(islot, out slot) && slot.Item1 == id && slot.Item2 == amount;
        }
        private int GetFreeSlot()
        {
            for (byte i = 0; i < _data.InvSlots; i++)
                if (!_items.ContainsKey(i))
                    return i;
            return -1;
        }
        private Tuple<int, int> GetSlot(byte slot)
        {
            Tuple<int, int> ret;
            return _items.TryGetValue(slot, out ret) ? ret : Empty;
        }
        private int AddItem(DB_Item item, int amount)
        {
            if (_itemsHash.Contains(item.ID) && (item.Flags & ItemFlags.Stackable) > 0)
            {
                var slots = _items.Where(x => (x.Value.Item1 == item.ID) && (x.Value.Item2 < item.Stack)).ToArray();
                foreach (var slot in slots)
                {
                    if (amount == 0) break;
                    amount = AddSlot(slot.Key, item, amount);
                }
                if (amount > 0) return AddNewSlots(item, amount);
            }
            else
                return AddNewSlots(item, amount);
            return 0;
        }
        private int AddNewSlots(DB_Item item, int amount)
        {
            var slot = GetFreeSlot();
            if (slot == -1) return -1;
            if (!_itemsHash.Contains(item.ID)) _itemsHash.Add(item.ID);
            while ((amount = SetSlot((byte)slot, item, amount)) > 0 && slot != -1)
                slot = GetFreeSlot();
            return amount;
        }
        private int RemoveSlot(byte slot, int id, int amount)
        {
            int slotAmount = _items[slot].Item2;
            if (slotAmount <= amount)
            {
                _items.Remove(slot);
                _view.DeleteItem(slot, slotAmount);
                return amount - slotAmount;
            }
            else
            {
                slotAmount -= amount;
                _items[slot] = new Tuple<int, int>(id, slotAmount);
                _view.DeleteItem(slot, amount);
                return 0;
            }
        }
        private int AddItem(byte slot, DB_Item item, int amount)
        {
            if (_items.ContainsKey(slot)) return amount;
            if (!_itemsHash.Contains(item.ID)) _itemsHash.Add(item.ID);
            return SetSlot(slot, item, amount);
        }
        private int SetSlot(byte slot, DB_Item item, int amount)
        {
            if ((item.Flags & ItemFlags.Stackable) == 0)
            {
                _items[slot] = new Tuple<int, int>(item.ID, 1);
                _view.AddItem(item.ID, 1, slot);
                return --amount;
            }
            else
            {
                int added = amount < item.Stack ? amount : item.Stack;
                _items[slot] = new Tuple<int, int>(item.ID, added);
                _view.AddItem(item.ID, added, slot);
                return amount - added;
            }
        }
        private int AddSlot(byte slot, DB_Item item, int amount)
        {
            if ((item.Flags & ItemFlags.Stackable) == 0) return amount;
            amount += _items[slot].Item2;
            int slotAmount = amount < item.Stack ? amount : item.Stack;
            _items[slot] = new Tuple<int, int>(item.ID, slotAmount);
            _view.AddItem(item.ID, slotAmount, slot);
            return amount - slotAmount;
        }
        #region RPC Handlers
        [Rpc(4)]//Worn Items
        private void RPC_004(NetMessage arg1, NetMessageInfo arg2)
        {
            _view.WornItems(arg2.Sender, _data);
        }
        [Rpc(6)]//Add item
        private void RPC_006(NetMessage arg1, NetMessageInfo arg2)
        {
            if (arg2.Sender.Id != _player.Id) return;
            DB_Item item;
            int itemID = arg1.ReadInt32();
            int amount = arg1.ReadInt32();
            if (_mPlayer.User.Access >= AccessLevel.TeamMember)
            {
                if (DataMgr.Select(itemID, out item))
                {
                    itemID = AddItem(item, amount);
                    if (itemID != 0)
                        _player.Announce($"Inventory full, added {(itemID == -1 ? 0 : amount - itemID)}/{amount} items");
                    else _player.Announce($"Added item {item.Name ?? item.ID.ToString()} amount {amount}");
                }
                else _player.Error($"Item {itemID} not found");
            }
            else _player.Error($"You haven't permission to adding items");
        }
        [Rpc(7)]//Remove item
        private void RPC_007(NetMessage arg1, NetMessageInfo arg2)
        {
            if (arg2.Sender.Id != _player.Id) return;
            byte islot = arg1.ReadByte();
            int amount = arg1.ReadInt32();
            var itemSlot = GetSlot(islot);
            if (itemSlot.Item1 != -1)
            {
                int ramount = RemoveItems(itemSlot.Item1, amount);
                if (ramount == 0)
                    _player.Announce($"Removed {amount} items {itemSlot.Item1} from {islot}");
                else _player.Announce($"Error while removing items {itemSlot.Item1} from {islot} removed {amount - ramount}/{amount}");
            }
            else _player.Error($"Inventory slot {islot} is empty");
        }
        [Rpc(8)]//Wear item
        private void RPC_008(NetMessage arg1, NetMessageInfo arg2)
        {
            if (arg2.Sender.Id != _player.Id) return;
            DB_Item item;
            byte wslot = arg1.ReadByte();
            byte islot = arg1.ReadByte();
            var itemSlot = GetSlot(islot);
            ItemSlot wslotType = wslot.ToItemSlot();
            if (itemSlot.Item1 != -1 && DataMgr.Select(itemSlot.Item1, out item))
            {
                if ((item.Slot & wslotType) == wslotType)
                {
                    _items.Remove(islot);
                    _view.DeleteItem(islot, 1);
                    if (_wears.ContainsKey(wslot))
                        AddItem(DataMgr.SelectItem(_wears[wslot]), 1);
                    _wears[wslot] = item.ID;
                    if ((item.Flags & ItemFlags.Stats) > 0)
                        _mPlayer.Stats.UpdateStats();
                    _view.WearItem(item.ID, wslot);
                }
                else _player.Error($"You can't whear item {item.Name ?? item.ID.ToString()} in slot {wslotType}");
            }
            else _player.Error($"Inventory slot {islot} is empty");
        }
        [Rpc(9)]//Unwear item
        private void RPC_009(NetMessage arg1, NetMessageInfo arg2)
        {
            if (arg2.Sender.Id != _player.Id) return;
            DB_Item item; int itemSlot;
            byte wslot = arg1.ReadByte();
            byte islot = arg1.ReadByte();
            byte wearSlot = (byte)(wslot + 1);
            if (!_wears.ContainsKey(wearSlot)) return;
            if (GetSlot(islot).Item1 != -1) itemSlot = GetFreeSlot(); else itemSlot = islot;
            if (itemSlot != -1 && DataMgr.Select(_wears[wearSlot], out item))
            {
                if (AddItem((byte)itemSlot, item, 1) == 0)
                {
                    _wears.Remove(wearSlot);
                    _view.UnwearItem(wslot);
                    if ((item.Flags & ItemFlags.Stats) > 0)
                        _mPlayer.Stats.UpdateStats();
                }
                else
                    _view.WearItem(item.ID, wearSlot);
            }
            else
                _view.WearItem(_wears[wearSlot], wearSlot);
        }
        [Rpc(12)]//Use item
        private void RPC_012(NetMessage arg1, NetMessageInfo arg2)
        {
            DB_Item item;
            byte islot = arg1.ReadByte();
            var itemSlot = GetSlot(islot);
            if (itemSlot.Item1 != -1 && DataMgr.Select(itemSlot.Item1, out item))
            {
                if ((item.Flags & ItemFlags.Usable) > 0)
                    ItemsScript.Use(item.ID, _mPlayer);
                else _player.Error($"You can't use item {item.Name ?? item.ID.ToString()}");
            }
            else _player.Error($"Inventory slot {islot} is empty or item not found");
        }
        [Rpc(20)]//Move item
        private void RPC_020(NetMessage arg1, NetMessageInfo arg2)
        {
            if (arg2.Sender.Id != _player.Id) return;
            DB_Item item01;
            byte slot01 = (byte)arg1.ReadInt32();
            byte slot02 = (byte)arg1.ReadInt32();
            var itemSlot01 = GetSlot(slot01);
            var itemSlot02 = GetSlot(slot02);
            if (itemSlot01.Item1 == -1 || itemSlot02.Item1 != -1) return;
            if (DataMgr.Select(itemSlot01.Item1, out item01))
            {
                _items.Remove(slot01);
                SetSlot(slot02, item01, itemSlot01.Item2);
            }
        }
        #endregion
        #region Events Handlers
        private void ItemsMgr_OnSpawn()
        {
            _view = _wPlayer.View;
            _view.SubscribeMarkedRpcsOnComponent(this);
        }
        private void ItemsMgr_OnDestroy()
        {
            _view = null;
            _data = null;
            _wears = null;
            _items = null;
            _player = null;
            _mPlayer = null;
            _wPlayer = null;
            _itemsHash = null;
        }
        #endregion
    }
}