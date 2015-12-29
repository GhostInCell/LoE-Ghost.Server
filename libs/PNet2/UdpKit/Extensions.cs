#if UDPKIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UdpKit;

namespace PNet.UdpKit
{
    static class Extensions
    {
#pragma warning disable 618
        public static UdpEndPoint ConvertEndPoint(this IPEndPoint endpoint)
        {

            return new UdpEndPoint(new UdpIPv4Address(endpoint.Address.Address), (ushort)endpoint.Port);
        }

        public static IPEndPoint ConvertEndPoint(this UdpEndPoint endpoint)
        {
            long netOrder = IPAddress.HostToNetworkOrder((int)endpoint.Address.Packed);
            var cAddress = new IPAddress(0L);
            cAddress.Address = netOrder;
            var cEndpoint = new IPEndPoint(cAddress, endpoint.Port);
            return cEndpoint;
        }
#pragma warning restore 618
    }
}
#endif