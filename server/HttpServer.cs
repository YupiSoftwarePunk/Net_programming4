using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public static class HttpServer
    {
        private static HttpListener listener = new HttpListener();

        public static void Start()
        {
            listener.Prefixes.Add("http://+:5000/api/");
            listener.Start();
            Console.WriteLine($"HTTP сервер запущен на порту 5000");

            Task.Run(async () =>
            {
                while (true)
                {
                    var ctx = await listener.GetContextAsync();
                    await HandleRequest(ctx);
                }
            });
        }

        private static async Task HandleRequest(HttpListenerContext ctx)
        {
            string path = ctx.Request.Url.AbsolutePath.ToLower();

            if (path.EndsWith("/users"))
            {
                var users = UDPServer.GetUsers();
                string json = System.Text.Json.JsonSerializer.Serialize(users);
                await WriteResponse(ctx, json);
            }
            else if (path.EndsWith("/stats"))
            {
                var stats = new
                {
                    UsersCount = UDPServer.GetUsers().Count,
                    MessagesCount = TcpServer.MessagesCount() 
                };
                string json = System.Text.Json.JsonSerializer.Serialize(stats);
                await WriteResponse(ctx, json);
            }
            else
            {
                await WriteResponse(ctx, "{\"error\":\"unknown endpoint\"}", 404);
            }
        }

        private static async Task WriteResponse(HttpListenerContext ctx, string body, int status = 200)
        {
            ctx.Response.StatusCode = status;
            byte[] buffer = Encoding.UTF8.GetBytes(body);
            ctx.Response.ContentType = "application/json";
            await ctx.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            ctx.Response.Close();
        }

    }
}
