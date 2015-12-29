#if UDPKIT
using UdpKit;

namespace PNet.UdpKit
{
    class NetMessageSerializer : UdpSerializer<NetMessage>
    {
        public override bool Pack(UdpStream stream, NetMessage input, out NetMessage sent)
        {
            stream.WriteByteArray(input.Data, 0, input.LengthBytes);
            sent = input;
            return true;
        }

        public override bool Unpack(UdpStream stream, out NetMessage received)
        {
            var readOffset = UdpMath.BytesRequired(stream.Position);
            var readLength = UdpMath.BytesRequired(stream.Size - stream.Position);

            // allocate a new stream and copy data
            received = NetMessage.GetMessage(readLength);
            received.Write(stream.ByteBuffer, readOffset, readLength);
            received.Position = 0;
            return true;
        }
    }
}
#endif