using PNetR;

namespace Ghost.Server.Utilities.Interfaces
{
    public interface INetworkManager
    {
        NetworkView View
        {
            get;
        }

        void UpdateView(NetworkView view);
    }
}