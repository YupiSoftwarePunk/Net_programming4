using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public class UDPServer
    {
        private static readonly List<ClientInfo> clients = new();
        private static readonly object locker = new();
        private static UdpClient? udpServer;
        private static int port;

        public static void Start(int port)
        {
            try
            {
                udpServer = new UdpClient(port);
                _ = Task.Run(ListenAsync);
                Console.WriteLine($"UDP сервер уведомлений запущен на порту {port}...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка запуска UDP сервера: {ex.Message}");
            }
        }

        private static async Task ListenAsync()
        {
            while (true)
            {
                var result = await udpServer!.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer);
                await HandleMessage(result.RemoteEndPoint, message);
            }
        }

        private static async Task HandleMessage(IPEndPoint sender, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            if (message.StartsWith("HELLO:"))
            {
                string name = message.Substring(6).Trim();
                lock (locker)
                {
                    clients.RemoveAll(c => c.Name == name);
                    clients.Add(new ClientInfo(name, sender));
                }

                Console.WriteLine($"{name} подключился");
                await Broadcast($"{name} ONLINE");
                await SendUsers();
            }
            else if (message.StartsWith("BYE:"))
            {
                string name = message.Substring(4).Trim();
                lock (locker)
                {
                    clients.RemoveAll(c => c.Name == name);
                }

                Console.WriteLine($"{name} отключился");
                await Broadcast($"{name} OFFLINE");
                await SendUsers();
            }
        }

        private static async Task Broadcast(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            List<ClientInfo> snapshot;
            lock (locker) snapshot = clients.ToList();

            foreach (var client in snapshot)
            {
                await udpServer!.SendAsync(data, data.Length, client.EndPoint);
            }
        }

        private static async Task SendUsers()
        {
            List<ClientInfo> snapshot;
            lock (locker) snapshot = clients.ToList();

            string usersList = "USERS:" + string.Join(",", snapshot.Select(c => c.Name));
            await Broadcast(usersList);
        }

        public static List<ClientInfo> GetUsers()
        {
            lock (locker)
            {
                return clients.ToList();
            }
        }
    }
}
