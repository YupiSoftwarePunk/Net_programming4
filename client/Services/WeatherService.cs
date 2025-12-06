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
    public class WeatherService
    {
        private readonly HttpClient httpClient;
        private readonly string apiKey;

        public WeatherService(string apiKey)
        {
            httpClient = new HttpClient();
            this.apiKey = apiKey;
        }

        public async Task<WeatherInfo?> GetWeatherAsync(string city)
        {
            try
            {
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=ru";
                var response = await httpClient.GetStringAsync(url);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var weather = JsonSerializer.Deserialize<WeatherApiResponse>(response, options);

                if (weather == null || weather.Main == null || weather.Weather.Length == 0)
                    return null;

                return new WeatherInfo
                {
                    City = weather.Name,
                    Temp = weather.Main.Temp,
                    Description = weather.Weather[0].Description
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка получения погоды: " + ex.Message);
                return null;
            }
        }
    }
}
