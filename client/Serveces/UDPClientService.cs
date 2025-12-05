using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace client.Serveces
{
    public class UDPClientService
    {
        private readonly UdpClient udpClient;
        private readonly IPEndPoint serverEndPoint;
        private readonly string userName;

        public event Action<string>? NotificationReceived;
        public event Action<int>? OnlineCountChanged;

        public UDPClientService(string serverIp, int port, string userName)
        {
            udpClient = new UdpClient();
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), port);
            this.userName = userName;
        }

        public async Task ConnectAsync()
        {
            await SendRawMessage($"HELLO:{userName}");
            _ = Task.Run(ReceiveMessage);
        }

        public async Task DisconnectAsync()
        {
            await SendRawMessage($"BYE:{userName}");
            udpClient.Close();
        }

        private async Task SendRawMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await udpClient.SendAsync(data, data.Length, serverEndPoint);
        }

        private async Task ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync();
                    string msg = Encoding.UTF8.GetString(result.Buffer);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (msg.StartsWith("USERS:"))
                        {
                            var users = msg.Substring(6).Split(',', StringSplitOptions.RemoveEmptyEntries);
                            OnlineCountChanged?.Invoke(users.Length);
                        }
                        else
                        {
                            NotificationReceived?.Invoke(msg);
                        }
                    });
                }
                catch
                {
                    break;
                }
            }
        }
    }
}
