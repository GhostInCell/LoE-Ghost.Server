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
    public class WO_Switch : ServerObject
    {
        private readonly DB_Map m_map;
        private NetworkedSceneObjectView m_view;

        public override byte TypeID
        {
            get
            {
                return Constants.TypeIDSwitch;
            }
        }

        public WO_Switch(DB_WorldObject data, ObjectsMgr manager)
            : base(data, manager)
        {
            if (!DataMgr.Select(data.Data01, out m_map))
                ServerLogger.LogError($"Map Switch {data.Guid} map {data.Data01} doesn't exist");
            else
            {
                m_view = _server.Room.SceneViewManager.CreateNetworkedSceneObjectView(m_data.Guid);
                m_view.SubscribeToRpc(1, RPC_001);
                OnDestroy += WO_Switch_OnDestroy;
            }
            Spawn();
        }
        #region Events Handlers
        private void WO_Switch_OnDestroy()
        {
            m_view.UnsubscribeFromRpc(1);
            m_view = null;
        }
        #endregion
        private async void RPC_001(NetMessage arg1, NetMessageInfo arg2)
        {
            MapPlayer player = _server[arg2.Sender.Id];
            if (player == null)
            {
                ServerLogger.LogWarning($"Switch from map {m_data.Data01} on portal {m_data.Guid} failed: player {arg2.Sender.Id} not found!");
                return;
            }
            if (Vector3.DistanceSquared(m_data.Position, player.Object.Position) <= Constants.MaxInteractionDistanceSquared)
            {
                if (await player.PrepareForMapSwitch())
                {
                    player.User.Spawn = (ushort)m_data.Data02;
                    arg2.Sender.SynchNetData();
                    arg2.Sender.ChangeRoom(m_map.Name);
                    player = null;
                }
                else
                {
                    player.CreateSaveTimer();
                    ServerLogger.LogWarning($"Switch from map {m_data.Data01} on portal {m_data.Guid} failed: couldn't save player {arg2.Sender.Id} character!");
                    player.SystemMsg("Map switch failed: couldn't save character to database!");
                }
            }
            else
                player.SystemMsg("You far away from the portal.");
        }
    }
}