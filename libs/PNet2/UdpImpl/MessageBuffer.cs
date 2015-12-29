#if UDP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace PNet.UdpImpl
{
    internal struct MessageBuffer
    {
        public int Size;
        public IPEndPoint Sender;
        public Byte[] Buffer;

        public MessageBuffer(IPEndPoint sender, int size, byte[] buffer)
        {
            Sender = sender;
            Size = size;
            Buffer = buffer;
        }
    }
}
#endif