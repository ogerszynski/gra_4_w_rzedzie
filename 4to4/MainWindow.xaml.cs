using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Connect4
{
    public partial class MainWindow : Window
    {
        private int[,] board; // 0 - empty, 1 - player 1, 2 - player 2
        private int currentPlayer;
        private const int Rows = 6;
        private const int Columns = 7;

        private Server? server;
        private Client? client;
        private bool isServerRunning = false;
        private bool isClientConnected = false;
        private int clientPlayerNumber = 0;

        public MainWindow()
        {
            InitializeComponent();
            board = new int[Rows, Columns];
            NewGame();
        }

        private void NewGame()
        {
            board = new int[Rows, Columns];
            currentPlayer = 1;
            DrawBoard();
        }

        private void DrawBoard()
        {
            BoardGrid.Children.Clear();
            BoardGrid.RowDefinitions.Clear();
            BoardGrid.ColumnDefinitions.Clear();

            for (int row = 0; row < Rows; row++)
            {
                BoardGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int col = 0; col < Columns; col++)
            {
                BoardGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    var cell = new Border
                    {
                        Background = new SolidColorBrush(Colors.LightGray),
                        BorderBrush = new SolidColorBrush(Colors.Black),
                        BorderThickness = new Thickness(1)
                    };
                    cell.MouseLeftButtonDown += Cell_MouseLeftButtonDown;
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);
                    BoardGrid.Children.Add(cell);
                }
            }
        }

        private void UpdateBoard(int row, int col, int player)
        {
            board[row, col] = player;
            var ellipse = new System.Windows.Shapes.Ellipse
            {
                Fill = player == 1 ? Brushes.Red : Brushes.Yellow
            };
            Grid.SetRow(ellipse, row);
            Grid.SetColumn(ellipse, col);
            BoardGrid.Children.Add(ellipse);
        }

        private async void Cell_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!isServerRunning && !isClientConnected)
            {
                MessageBox.Show("Server is not running or client is not connected!");
                return;
            }

            if (currentPlayer != clientPlayerNumber)
            {
                MessageBox.Show("It is not your turn!");
                return;
            }

            var cell = (Border)sender;
            int col = Grid.GetColumn(cell);

            for (int row = Rows - 1; row >= 0; row--)
            {
                if (board[row, col] == 0)
                {
                    board[row, col] = currentPlayer;
                    var ellipse = new System.Windows.Shapes.Ellipse
                    {
                        Fill = currentPlayer == 1 ? Brushes.Red : Brushes.Yellow
                    };
                    Grid.SetRow(ellipse, row);
                    Grid.SetColumn(ellipse, col);
                    BoardGrid.Children.Add(ellipse);

                    string message = $"{row},{col},{currentPlayer}";
                    if (server != null)
                    {
                        await server.SendMessageAsync(message);
                    }
                    else if (client != null)
                    {
                        await client.SendMessageAsync(message);
                    }

                    if (CheckWin(row, col))
                    {
                        MessageBox.Show($"Player {currentPlayer} wins!");
                        NewGame();
                        return;
                    }

                    currentPlayer = currentPlayer == 1 ? 2 : 1;
                    break;
                }
            }
        }

        private bool CheckWin(int row, int col)
        {
            return CheckDirection(row, col, 1, 0) || // horizontal
                   CheckDirection(row, col, 0, 1) || // vertical
                   CheckDirection(row, col, 1, 1) || // diagonal /
                   CheckDirection(row, col, 1, -1);  // diagonal \
        }

        private bool CheckDirection(int row, int col, int dRow, int dCol)
        {
            int count = 1;
            for (int i = 1; i < 4; i++)
            {
                int r = row + i * dRow;
                int c = col + i * dCol;
                if (r < 0 || r >= Rows || c < 0 || c >= Columns || board[r, c] != currentPlayer)
                    break;
                count++;
            }

            for (int i = 1; i < 4; i++)
            {
                int r = row - i * dRow;
                int c = col - i * dCol;
                if (r < 0 || r >= Rows || c < 0 || c >= Columns || board[r, c] != currentPlayer)
                    break;
                count++;
            }

            return count >= 4;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }

        private async void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (server == null)
            {
                server = new Server();
                StatusTextBlock.Text = "Starting server...";
                LogMessage("Starting server...");
                try
                {
                    await server.StartAsync(OnMessageReceived);
                    isServerRunning = true;
                    StatusTextBlock.Text = "Server started!";
                    LogMessage("Server started!");

                    // Display the server IP address
                    string serverIp = GetLocalIPAddress();
                    LogMessage($"Server IP: {serverIp}");
                    StatusTextBlock.Text = $"Server started on {serverIp}:{NetworkUtils.Port}";
                }
                catch (Exception ex)
                {
                    isServerRunning = false;
                    StatusTextBlock.Text = $"Failed to start server: {ex.Message}";
                    LogMessage($"Failed to start server: {ex.Message}");
                }
            }
            else
            {
                StatusTextBlock.Text = "Server is already running!";
                LogMessage("Server is already running!");
            }
        }

        private async void ConnectToServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (client == null)
            {
                client = new Client();
                string serverIp = ServerIpTextBox.Text;
                StatusTextBlock.Text = "Connecting to server...";
                LogMessage("Connecting to server...");
                try
                {
                    await client.ConnectAsync(serverIp);
                    isClientConnected = true;
                    StatusTextBlock.Text = "Connected to server!";
                    LogMessage("Connected to server!");

                    // Assign player number
                    clientPlayerNumber = await client.ReceivePlayerNumberAsync();
                    LogMessage($"You are player {clientPlayerNumber}");

                    // Automatically update the board three times
                    for (int i = 0; i < 3; i++)
                    {
                        await Task.Delay(1000); // 1 second delay between updates
                        await UpdateBoardAsync();
                    }

                    // Start listening for messages
                    _ = ListenForMessagesAsync();
                }
                catch (Exception ex)
                {
                    isClientConnected = false;
                    StatusTextBlock.Text = $"Failed to connect to server: {ex.Message}";
                    LogMessage($"Failed to connect to server: {ex.Message}");
                }
            }
            else
            {
                StatusTextBlock.Text = "Client is already connected!";
                LogMessage("Client is already connected!");
            }
        }

        private async void UpdateBoardButton_Click(object sender, RoutedEventArgs e)
        {
            await UpdateBoardAsync();
        }

        private async Task UpdateBoardAsync()
        {
            if (isClientConnected)
            {
                // Request move history from server
                await client.SendMessageAsync("REQUEST_HISTORY");

                // Clear current board
                NewGame();

                // Process existing moves
                string? message;
                while ((message = await client.ReceiveMessageAsync()) != null && message != "END_OF_HISTORY")
                {
                    OnMessageReceived(message);
                }
            }
            else
            {
                MessageBox.Show("Client is not connected to server!");
            }
        }

        private async Task ListenForMessagesAsync()
        {
            while (isClientConnected)
            {
                var message = await client.ReceiveMessageAsync();
                if (message != null)
                {
                    OnMessageReceived(message);
                }
            }
        }

        private void OnMessageReceived(string message)
        {
            LogMessage($"Message received: {message}");
            if (message == "ACK" || message == "END_OF_HISTORY") return; // Ignore ACK and END_OF_HISTORY messages

            string[] parts = message.Split(',');

            if (parts[0] == "TURN")
            {
                currentPlayer = int.Parse(parts[1]);
                LogMessage($"It's now player {currentPlayer}'s turn.");
                return;
            }
            if (parts[0] == "DISCONNECTED")
            {
                int playerNumber = int.Parse(parts[1]);
                LogMessage($"Player {playerNumber} disconnected.");
                return;
            }
            if (parts[0] == "RECONNECTED")
            {
                int playerNumber = int.Parse(parts[1]);
                LogMessage($"Player {playerNumber} reconnected.");
                return;
            }

            int row = int.Parse(parts[0]);
            int col = int.Parse(parts[1]);
            int player = int.Parse(parts[2]);

            UpdateBoard(row, col, player);

            if (CheckWin(row, col))
            {
                MessageBox.Show($"Player {player} wins!");
                NewGame();
            }

            currentPlayer = player == 1 ? 2 : 1;
        }

        private void LogMessage(string message)
        {
            LogTextBox.AppendText($"{DateTime.Now}: {message}\n");
            LogTextBox.ScrollToEnd();
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
