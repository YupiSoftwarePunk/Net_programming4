using System.Net;
using System.Net.Sockets;
using System.Text;

namespace server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var tcpListener = new TcpListener(IPAddress.Any, 8080);
            tcpListener.Start();
            Console.WriteLine("TCP сервер запущен...");


            UDPServer.Start(8082);            
            
            HttpServer.Start();

            while (true)
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                TcpServer.AddClient(tcpClient);
                Console.WriteLine("Клиент подключен (TCP)");

                _ = TcpServer.ReceiveMessages(tcpClient);
            }
        }
    }
}
