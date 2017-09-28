using PNet;
using System;

namespace PNetR
{
    /// <summary>
    /// An attribute to apply alongside the RpcAttribute to not run the rpc if the owner isn't the sender
    /// Filters only work on methods that aren't (NetMessage, [Optional]NetMessageInfo), and only return void
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OwnerOnlyAttribute : Attribute, IComponentRpcFilterProvider<NetMessageInfo>
    {
        public IRpcFilter<NetMessageInfo> GetFilter(IComponentInfoRpcProvider<NetMessageInfo> provider)
        {
            return new OwnerFilter((provider as NetworkView));
        }
    }

    internal class OwnerFilter : IRpcFilter<NetMessageInfo>
    {
        private NetworkView _view;

        public OwnerFilter(NetworkView view)
        {
            _view = view;
            _view.Destroyed += ViewOnDestroyed;
        }

        private void ViewOnDestroyed()
        {
            _view.Destroyed -= ViewOnDestroyed;
            _view = null;
        }

        public bool Filter(NetMessageInfo info)
        {
            if (_view == null)
                throw new Exception("OwnerFilter isn't cleaned up properly");
            return _view.Owner == info.Sender;
        }
    }
}
