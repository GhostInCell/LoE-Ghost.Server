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
using System.Numerics;

namespace Ghost.Server.Core.Objects
{
    public class WO_NPC : CreatureObject
    {
        private readonly DB_NPC _npc;
        private readonly DB_WorldObject _data;
        private SER_Shop shop_ser;
        private DialogScript _dialog;
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
        public WO_NPC(DB_WorldObject data, ObjectsMgr manager)
            : base(manager.GetNewGuid() | Constants.ReleaseGuide, manager)
        {
            _data = data;
            if (!DataMgr.Select(data.ObjectID, out _npc))
                ServerLogger.LogError($"NPC id {data.ObjectID} doesn't exist");
            if ((_npc.Flags & NPCFlags.Scripted) > 0)
                _dialog = _server.Dialogs.GetDialog(_npc.Dialog);
            if ((_npc.Flags & NPCFlags.Trader) > 0)
                shop_ser = new SER_Shop(_npc.Items);
            _movement = new NullMovement(this);
            Spawn();
        }
        public void CloseShop(MapPlayer player)
        {
            player.Shop = null;
            _view.Rpc(6, 23, player.Player);
        }
        #region RPC Handlers
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
                player.Shop = this;
                _view.Rpc(6, 22, arg2.Sender, shop_ser, $"{_npc.Pony.Name}'s shop");
            }
        }
        private void RPC_06_23(NetMessage arg1, NetMessageInfo arg2)
        {
            MapPlayer player = _server[arg2.Sender.Id];
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
            _view.FinishedInstantiation += View_FinishedInstantiation;
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
            _view.FinishedInstantiation -= View_FinishedInstantiation;
        }
        private void WO_NPC_OnDestroy()
        {
            if (_dialog != null) _dialog.NPC[_npc.DialogIndex] = null;
            _view.FinishedInstantiation -= View_FinishedInstantiation;
            _dialog = null;
            shop_ser = null;
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