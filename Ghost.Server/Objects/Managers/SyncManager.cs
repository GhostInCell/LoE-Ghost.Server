using PNet;
using PNetR;

namespace Ghost.Server.Objects.Managers
{
    [NetComponent(2)]
    public class SyncManager : NetworkManager<MovableObject>
    {
        #region RPC Handlers
        [Rpc(196, false)]//PurchaseSkillUpgrade
        private void RPC_196(NetMessage message, NetMessageInfo info)
        {

        }

        [Rpc(201, false)]//Summon
        private void RPC_201(NetMessage message, NetMessageInfo info)
        {

        }

        [Rpc(202, false)]//Animation
        private void RPC_202(NetMessage message, NetMessageInfo info)
        {

        }

        [Rpc(205, false)]//Hide
        private void RPC_205(NetMessage message, NetMessageInfo info)
        {

        }
        #endregion
        #region Overridden Methods
        protected override void OnViewCreated()
        {
            base.OnViewCreated();
            m_view.SubscribeMarkedRpcsOnComponent(this);
        }
        #endregion
    }
}