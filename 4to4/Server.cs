using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Connect4
{
    public class Server
    {
        private TcpListener listener;
        private List<TcpClient> connectedClients = new List<TcpClient>();
        private Dictionary<int, TcpClient> playerClients = new Dictionary<int, TcpClient>();
        private List<string> moveHistory = new List<string>();
        private int currentPlayer = 1;

        public Server()
        {
            listener = new TcpListener(IPAddress.Any, NetworkUtils.Port);
        }

        public async Task StartAsync(Action<string> onMessageReceived)
        {
            listener.Start();
            try
            {
                Console.WriteLine("Server is listening...");
                while (true)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    connectedClients.Add(client);
                    int playerNumber = GetAvailablePlayerNumber();
                    playerClients[playerNumber] = client;
                    Console.WriteLine($"Client connected as player {playerNumber}.");
                    await NetworkUtils.WriteStringAsync(client.GetStream(), playerNumber.ToString());

                    // Send move history to the new client
                    foreach (var move in moveHistory)
                    {
                        await NetworkUtils.WriteStringAsync(client.GetStream(), move);
                    }
                    await NetworkUtils.WriteStringAsync(client.GetStream(), "END_OF_HISTORY");

                    // Notify current player to new client
                    await NetworkUtils.WriteStringAsync(client.GetStream(), $"TURN,{currentPlayer}");

                    // Inform other clients about reconnection
                    foreach (var connectedClient in connectedClients)
                    {
                        if (connectedClient != client)
                        {
                            await NetworkUtils.WriteStringAsync(connectedClient.GetStream(), $"RECONNECTED,{playerNumber}");
                        }
                    }

                    _ = HandleClientAsync(client, onMessageReceived, playerNumber);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StartAsync: {ex.Message}");
                throw;
            }
        }

        private int GetAvailablePlayerNumber()
        {
            for (int i = 1; i <= 2; i++)
            {
                if (!playerClients.ContainsKey(i))
                {
                    return i;
                }
            }
            return playerClients.Count + 1;
        }

        private async Task HandleClientAsync(TcpClient client, Action<string> onMessageReceived, int playerNumber)
        {
            var stream = client.GetStream();
            try
            {
                while (true)
                {
                    var message = await NetworkUtils.ReadStringAsync(stream);
                    if (message != null && message != "ACK")
                    {
                        if (message == "REQUEST_HISTORY")
                        {
                            foreach (var move in moveHistory)
                            {
                                await NetworkUtils.WriteStringAsync(stream, move);
                            }
                            await NetworkUtils.WriteStringAsync(stream, "END_OF_HISTORY");
                        }
                        else
                        {
                            moveHistory.Add(message);
                            onMessageReceived(message);
                            foreach (var connectedClient in connectedClients)
                            {
                                if (connectedClient != client)
                                {
                                    await NetworkUtils.WriteStringAsync(connectedClient.GetStream(), message);
                                }
                            }
                            await NetworkUtils.WriteStringAsync(stream, "ACK");

                            // Update the current player and notify clients
                            currentPlayer = currentPlayer == 1 ? 2 : 1;
                            string currentPlayerMessage = $"TURN,{currentPlayer}";
                            foreach (var connectedClient in connectedClients)
                            {
                                await NetworkUtils.WriteStringAsync(connectedClient.GetStream(), currentPlayerMessage);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleClientAsync: {ex.Message}");
                connectedClients.Remove(client);
                playerClients.Remove(playerNumber);
                client.Close();

                // Notify remaining clients about disconnection
                foreach (var connectedClient in connectedClients)
                {
                    await NetworkUtils.WriteStringAsync(connectedClient.GetStream(), $"DISCONNECTED,{playerNumber}");
                }
            }
        }

        public async Task SendMessageAsync(string message)
        {
            moveHistory.Add(message);
            foreach (var client in connectedClients)
            {
                var stream = client.GetStream();
                await NetworkUtils.WriteStringAsync(stream, message);
            }
        }

        public void Stop()
        {
            listener.Stop();
        }
    }
}
