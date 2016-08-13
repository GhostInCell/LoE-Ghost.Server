using PNet;
using PNetR;

namespace Ghost.Server.Objects.Managers
{
    [NetComponent(7)]
    public class TradingManager : NetworkManager<PlayerObject>
    {
        public TradingManager()
            : base()
        {

        }
        #region RPC Handlers		
        [Rpc(1, false)]//Request
        private void RPC_004(NetMessage message, NetMessageInfo info)
        {

        }

        [Rpc(4, false)]//Cancel
        private void RPC_006(NetMessage message, NetMessageInfo info)
        {

        }

        [Rpc(6, false)]//Offer
        private void RPC_007(NetMessage message, NetMessageInfo info)
        {

        }

        [Rpc(8, false)]//Ready
        private void RPC_008(NetMessage message, NetMessageInfo info)
        {
        }

        [Rpc(9, false)]//UnReady
        private void RPC_009(NetMessage message, NetMessageInfo info)
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