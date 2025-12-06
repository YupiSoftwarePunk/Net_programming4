using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client.Models
{
    public class WeatherInfo
    {
        public string City { get; set; } = "";
        public double Temp { get; set; }
        public string Description { get; set; } = "";
    }
}
