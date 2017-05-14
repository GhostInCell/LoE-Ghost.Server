using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using PNet;
using PNetR;

namespace Ghost.Server.Objects.Instances
{
    public class SwitchTrigger : WorldObject
    {
        private DB_Map m_map;
        private DB_WorldObject m_data;
        private NetworkedSceneObjectView m_view;

        public SwitchTrigger(DB_WorldObject data)
            : base()
        {
            m_data = data;
            m_position = m_data.Position;
            m_rotation = m_data.Rotation;
        }
        #region RPC Handlers
        private async void RPC_001(NetMessage message, NetMessageInfo info)
        {
            if (m_manager.TryGet(info.Sender, out PlayerObject @object))
            {
                if (@object.CanInteractWith(this))
                {
                    if (await @object.PrepareForMapSwitch())
                    {
                        @object.User.Spawn = (ushort)m_data.Data02;
                        info.Sender.SynchNetData();
                        info.Sender.ChangeRoom(m_map.Name);
                    }
                }
            }
        }
        #endregion
        #region Overridden Methods
        protected override bool OnLoad()
        {
            return base.OnLoad() && DataMgr.Select(m_data.Data01, out m_map);
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            m_view = m_manager.CreateSceneObject(m_data.Guid);
            m_view.SubscribeToRpc(1, RPC_001);
        }
        #endregion
    }
}