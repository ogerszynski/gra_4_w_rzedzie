using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Connect4
{
    public class Client
    {
        private TcpClient client;

        public Client()
        {
            client = new TcpClient();
        }

        public async Task ConnectAsync(string serverIp)
        {
            await client.ConnectAsync(serverIp, NetworkUtils.Port);
        }

        public async Task<string?> ReceiveMessageAsync()
        {
            var stream = client.GetStream();
            return await NetworkUtils.ReadStringAsync(stream);
        }

        public async Task SendMessageAsync(string message)
        {
            var stream = client.GetStream();
            await NetworkUtils.WriteStringAsync(stream, message);
        }

        public async Task<int> ReceivePlayerNumberAsync()
        {
            var stream = client.GetStream();
            string? playerNumberString = await NetworkUtils.ReadStringAsync(stream);

            if (string.IsNullOrEmpty(playerNumberString))
            {
                throw new Exception("Received null or empty player number from server");
            }

            return int.Parse(playerNumberString);
        }
    }
}
