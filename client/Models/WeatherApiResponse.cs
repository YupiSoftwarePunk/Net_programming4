using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client.Models
{
    public class WeatherApiResponse
    {
        public WeatherMain Main { get; set; } = new();
        public WeatherDescription[] Weather { get; set; } = Array.Empty<WeatherDescription>();
        public string Name { get; set; } = "";
    }



    public class WeatherMain
    {
        public double Temp { get; set; }
    }

    public class WeatherDescription
    {
        public string Description { get; set; } = "";
    }
}
