using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCart.Utility
{
    public class API
    {
        public RazorAPIConfig RazorAPI { get; set; }
        public CountryAPIConfig CountryAPI { get; set; }
        public FlightAPIConfig FlightAPI { get; set; }
    }

    //List the new Api Models Down here and add them in the class API
    public class RazorAPIConfig
    {
        public string SecretKey { get; set; }
        public string? PublishableKey { get; set; }
    }

    public class CountryAPIConfig
    {
        public string countryApiUrl { get; set; }
        public string ApiKey { get; set; }
    }

    public class FlightAPIConfig
    {
        public string Url { get; set; }
        public string ApiKey { get; set; }
        public string APISecret { get; set; }
        public string CityUrl { get; set; }
    }

}