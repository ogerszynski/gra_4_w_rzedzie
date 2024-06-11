using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Connect4
{
    public static class NetworkUtils
    {
        public const int Port = 5000;

        public static async Task<string?> ReadStringAsync(NetworkStream stream)
        {
            using (var reader = new StreamReader(stream, leaveOpen: true))
            {
                return await reader.ReadLineAsync();
            }
        }

        public static async Task WriteStringAsync(NetworkStream stream, string message)
        {
            using (var writer = new StreamWriter(stream, leaveOpen: true))
            {
                await writer.WriteLineAsync(message);
                await writer.FlushAsync();
            }
        }
    }
}
