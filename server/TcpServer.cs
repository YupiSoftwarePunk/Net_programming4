using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public class TcpServer
    {
        private static readonly List<TcpClient> clients = new();
        private static readonly object locker = new();
        private static int messagesCount = 0;

        public static void AddClient(TcpClient client)
        {
            lock (locker)
            {
                clients.Add(client);
            }
        }

        private static void RemoveClient(TcpClient client)
        {
            lock (locker)
            {
                clients.Remove(client);
            }
        }

        public static async Task ReceiveMessages(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (read == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, read);
                    Console.WriteLine("Сообщение от клиента: " + message);
                    lock (locker)
                    {
                        messagesCount++;
                    }

                    await SendMessageAsync(client, message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при обработке клиента: " + ex.Message);
            }
            finally
            {
                RemoveClient(client);
                client.Close();
            }
        }

        public static async Task SendMessageAsync(TcpClient sender, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            List<TcpClient> snapshot;
            lock (locker) snapshot = clients.ToList();

            foreach (var client in snapshot)
            {
                if (client == sender) continue;

                try
                {
                    NetworkStream stream = client.GetStream();
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при отправке: {ex.Message}");
                }
            }
        }



        public static int MessagesCount()
        {
            lock (locker)
            {
                return messagesCount;
            }
        }
    }
}
