#if LIDGREN
using Lidgren.Network;
#endif

namespace PNet
{
    public enum ConnectionStatus
    {
        Disconnected = 0,
        Connecting,
        Connected,
        FailedToConnect,
        Disconnecting,
    }

    public static class ConnectionStatusEx
    {
#if LIDGREN
        public static ConnectionStatus ToPNet(this NetConnectionStatus status)
        {
            switch (status)
            {
                case NetConnectionStatus.Connected:
                    return ConnectionStatus.Connected;
                case NetConnectionStatus.Disconnected:
                    return ConnectionStatus.Disconnected;
                case NetConnectionStatus.Disconnecting:
                    return ConnectionStatus.Disconnecting;
                case NetConnectionStatus.InitiatedConnect:
                    return ConnectionStatus.Connecting;
                case NetConnectionStatus.ReceivedInitiation:
                    return ConnectionStatus.Connecting;
                case NetConnectionStatus.RespondedAwaitingApproval:
                    return ConnectionStatus.Connecting;
                case NetConnectionStatus.RespondedConnect:
                    return ConnectionStatus.Connecting;
                default:
                    return ConnectionStatus.Disconnected;
            }
        }
#endif
    }
}
