using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Movment;
using Ghost.Server.Core.Players;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using Ghost.Server.Mgrs.Map;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNet;
using PNetR;
using System.Collections.Generic;
using System.Numerics;

namespace Ghost.Server.Core.Objects
{
    public class WO_NPC : CreatureObject
    {
        private readonly DB_NPC _npc;
        private readonly DB_WorldObject _data;
        private MapPlayer _owner;
        private WO_NPC _original;
        private SER_Shop shop_ser;
        private SER_Wears wears_ser;
        private DialogScript _dialog;
        private Dictionary<byte, int> _wears;
        public DB_NPC NPC
        {
            get
            {
                return _npc;
            }
        }
        public DialogScript Dialog
        {
            get
            {
                return _dialog;
            }
        }
        public override byte TypeID
        {
            get
            {
                return Constants.TypeIDNPC;
            }
        }
        public override Vector3 SpawnPosition
        {
            get
            {
                return _data.Position;
            }
        }
        public override Vector3 SpawnRotation
        {
            get
            {
                return _data.Rotation;
            }
        }
        public WO_NPC(WO_NPC original, MapPlayer owner)
            : base(original._manager.GetNewGuid() | Constants.CRObject, original._manager)
        {
            _owner = owner;
            _original = original;
            _npc = original._npc;
            _data = original._data;
            if ((_npc.Flags & NPCFlags.Dialog) > 0)
                _dialog = _server.Dialogs.GetClone(_npc.Dialog, owner);
            if ((_npc.Flags & NPCFlags.Trader) > 0)
                shop_ser = new SER_Shop(_npc.Items, $"{_npc.Pony.Name}'s shop");
            if ((_npc.Flags & NPCFlags.Wears) > 0)
            {
                DB_Item entry; byte slot;
                _wears = new Dictionary<byte, int>();
                wears_ser = new SER_Wears(_wears);
                foreach (var item in _npc.Wears)
                    if (item > 0 && DataMgr.Select(item, out entry))
                    {
                        slot = entry.Slot.ToWearSlot();
                        if (_wears.ContainsKey(slot))
                            ServerLogger.LogWarn($"NPC id {_data.ObjectID} duplicate wear slot {entry.Slot}");
                        else _wears[slot] = item;
                    }
            }
            if ((_npc.Flags & NPCFlags.ScriptedMovement) > 0)
                _movement = new ScriptedMovement(original._movement as ScriptedMovement, this);
            else
                _movement = new NullMovement(this);
            Spawn();
        }
        public WO_NPC(DB_WorldObject data, ObjectsMgr manager)
            : base(manager.GetNewGuid() | Constants.ReleaseGuide, manager)
        {
            _data = data;
            if (!DataMgr.Select(data.ObjectID, out _npc))
                ServerLogger.LogError($"NPC id {data.ObjectID} doesn't exist");
            if ((_npc.Flags & NPCFlags.Dialog) > 0)
                _dialog = _server.Dialogs.GetDialog(_npc.Dialog);
            if ((_npc.Flags & NPCFlags.Trader) > 0)
                shop_ser = new SER_Shop(_npc.Items, $"{_npc.Pony.Name}'s shop");
            if ((_npc.Flags & NPCFlags.Wears) > 0)
            {
                DB_Item entry; byte slot;
                _wears = new Dictionary<byte, int>();
                wears_ser = new SER_Wears(_wears);
                foreach (var item in _npc.Wears)
                    if (item > 0 && DataMgr.Select(item, out entry))
                    {
                        slot = entry.Slot.ToWearSlot();
                        if (_wears.ContainsKey(slot))
                            ServerLogger.LogWarn($"NPC id {data.ObjectID} duplicate wear slot {entry.Slot}");
                        else _wears[slot] = item;
                    }
            }
            if ((_npc.Flags & NPCFlags.ScriptedMovement) > 0)
                _movement = new ScriptedMovement(_npc.Movement, this);
            else
                _movement = new NullMovement(this);
            Spawn();
        }
        public void Clone(MapPlayer player)
        {
            player.Clones[_npc.ID] = new WO_NPC(this, player);
            _view.RebuildVisibility();
        }
        public void CloseShop(MapPlayer player)
        {
            _movement.Unlock();
            player.Shop = null;
            _view.Rpc(6, 23, player.Player);
        }
        #region RPC Handlers
        private void RPC_07_04(NetMessage arg1, NetMessageInfo arg2)
        {
            if ((_npc.Flags & NPCFlags.Wears) > 0)
                _view.Rpc(7, 4, arg2.Sender, wears_ser);
            else
                _view.Rpc(7, 4, arg2.Sender, Constants.MaxWornItems, (byte)0);
        }
        private void RPC_04_53(NetMessage arg1, NetMessageInfo arg2)
        {
            _view.Rpc(4, 53, arg2.Sender, _npc.Level);
        }
        private void RPC_06_10(NetMessage arg1, NetMessageInfo arg2)
        {
            DB_Item item;
            int itemID = arg1.ReadInt32();
            int amount = arg1.ReadInt32();
            MapPlayer player = _server[arg2.Sender.Id];
            if (player.Shop == this && _npc.Items.Contains(itemID) && DataMgr.Select(itemID, out item)
                && player.Char.Data.Bits >= (item.Price * amount))
            {
                amount -= player.Items.AddItems(itemID, amount);
                player.Char.Data.Bits -= (item.Price * amount);
                player.View.SetBits(player.Char.Data.Bits);
            }
        }
        private void RPC_06_11(NetMessage arg1, NetMessageInfo arg2)
        {
            DB_Item item;
            int itemID = arg1.ReadInt32();
            int amount = arg1.ReadInt32();
            byte islot = arg1.ReadByte();
            MapPlayer player = _server[arg2.Sender.Id];
            if (player.Shop == this && DataMgr.Select(itemID, out item) && (item.Flags & ItemFlags.Salable) > 0 && player.Items.HasInSlot(islot, itemID, amount))
            {
                player.Items.ClearSlot(islot);
                player.Char.Data.Bits += (item.Price * amount);
                player.View.SetBits(player.Char.Data.Bits);
            }
        }
        private void RPC_06_22(NetMessage arg1, NetMessageInfo arg2)
        {
            MapPlayer player = _server[arg2.Sender.Id];
            if ((_npc.Flags & NPCFlags.Trader) > 0 && Vector3.Distance(player.Object.Position, _movement.Position) <= Constants.MaxInteractionDistance)
            {
                _movement.Lock(false);
                _movement.LookAt(player.Object);
                player.Shop = this;
                _view.Rpc(6, 22, arg2.Sender, shop_ser);
            }
        }
        private void RPC_06_23(NetMessage arg1, NetMessageInfo arg2)
        {
            MapPlayer player = _server[arg2.Sender.Id];
            _movement.Unlock();
            player.Shop = null;
            _view.Rpc(6, 23, arg2.Sender);
        }
        private void RPC_10_49(NetMessage arg1, NetMessageInfo arg2)
        {
            MapPlayer player = _server[arg2.Sender.Id];
            if (player.Dialog == null)
            {
                player.Dialog = this;
                player.DialogBegin();
            }
        }
        #endregion
        #region Events Handlers
        private void WO_NPC_OnSpawn()
        {
            _view = _server.Room.Instantiate("PlayerBase", _movement.Position, _movement.Rotation);
            _view.CheckVisibility += View_CheckVisibility;
            _view.FinishedInstantiation += View_FinishedInstantiation;
            _view.SubscribeToRpc(7, 04, RPC_07_04);
            _view.SubscribeToRpc(4, 53, RPC_04_53);
            if ((_npc.Flags & NPCFlags.Trader) > 0)
            {
                _view.SubscribeToRpc(6, 10, RPC_06_10);
                _view.SubscribeToRpc(6, 11, RPC_06_11);
                _view.SubscribeToRpc(6, 22, RPC_06_22);
                _view.SubscribeToRpc(6, 23, RPC_06_23);
            }
            if (_dialog != null)
            {
                _dialog.NPC[_npc.DialogIndex] = this;
                _view.SubscribeToRpc(10, 49, RPC_10_49);
            }
        }
        private void WO_NPC_OnDespawn()
        {
            if (_dialog != null) _dialog.NPC[_npc.DialogIndex] = null;
            _view.CheckVisibility -= View_CheckVisibility;
            _view.FinishedInstantiation -= View_FinishedInstantiation;
        }
        private void WO_NPC_OnDestroy()
        {
            if (_dialog != null)
            {
                _dialog.NPC[_npc.DialogIndex] = null;
                if (_owner != null)
                    _server.Dialogs.RemoveClone(_dialog.ID, _owner);
            }
            if (_owner != null) _owner.Clones.Remove(_npc.ID);
            _view.CheckVisibility -= View_CheckVisibility;
            _view.FinishedInstantiation -= View_FinishedInstantiation;
            if (_original != null && _original.IsSpawned)
                _original._view.RebuildVisibility();
            _dialog = null;
            shop_ser = null;
        }
        private bool View_CheckVisibility(Player arg)
        {
            MapPlayer player = _server[arg.Id];
            return _owner != null ? arg.Id == _owner.Player.Id : player == null || !player.Clones.ContainsKey(_npc.ID);
        }
        private void View_FinishedInstantiation(Player obj)
        {
            if ((_npc.Flags & NPCFlags.Trader) > 0)
                _view.Rpc(2, 120, obj);
            _view.Rpc(2, 200, obj, _npc.Pony, _npc.Level, _npc.ID);
        }
        #endregion
    }
}