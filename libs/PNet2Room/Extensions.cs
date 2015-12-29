using System.Net.Sockets;
using System.Threading.Tasks;

namespace PNetR
{
    static class Extensions
    {
        public static Task<int> ReadAsync(this NetworkStream stream, byte[] buffer, int offset, int count)
        {
            return Task<int>.Factory.FromAsync(stream.BeginRead, stream.EndRead, buffer, offset, count, null);
        }

        public static Task WriteAsync(this NetworkStream stream, byte[] buffer, int offset, int size)
        {
            return Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, buffer, offset, size, null);
        }
    }
}
