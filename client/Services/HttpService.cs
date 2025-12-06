using client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace client.Services
{
    public class HttpService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;

        public HttpService(string baseUrl)
        {
            this.baseUrl = baseUrl.TrimEnd('/');
            httpClient = new HttpClient();
        }

        public async Task<ChatStats?> GetStatsAsync()
        {
            try
            {
                string url = $"{baseUrl}/stats";
                var response = await httpClient.GetStringAsync(url);
                return JsonSerializer.Deserialize<ChatStats>(response);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string[]?> GetUsersAsync()
        {
            try
            {
                string url = $"{baseUrl}/users";
                var response = await httpClient.GetStringAsync(url);
                return JsonSerializer.Deserialize<string[]>(response);
            }
            catch
            {
                return null;
            }
        }
    }
}
