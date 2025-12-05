using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;

namespace client.Serveces
{
    class TcpClientService
    {
        private readonly TcpClient tcpClient;
        private NetworkStream? stream;
        private readonly string userName;

        public event Action<string>? MessageReceived;

        public TcpClientService(string serverIp, int port, string userName)
        {
            tcpClient = new TcpClient();
            this.userName = userName;
            tcpClient.Connect(serverIp, port);
            stream = tcpClient.GetStream();
            _ = Task.Run(ReceiveMessages);
        }

        public async Task SendMessageAsync(string message)
        {
            if (stream == null) return;

            string formatted = $"MSG:{userName}:{message}";
            byte[] data = Encoding.UTF8.GetBytes(formatted);
            await stream.WriteAsync(data, 0, data.Length);
        }

        private async Task ReceiveMessages()
        {
            if (stream == null) return;

            byte[] buffer = new byte[1024];
            while (true)
            {
                try
                {
                    int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (read == 0) break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, read);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageReceived?.Invoke(msg);
                    });
                }
                catch
                {
                    break;
                }
            }
        }

        public void Disconnect()
        {
            stream?.Close();
            tcpClient.Close();
        }
    }
}
