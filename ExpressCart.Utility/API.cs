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
    }

    public class RazorAPIConfig
    {
        public string SecretKey { get; set; }
        public string PublishableKey { get; set; }
    }

    public class CountryAPIConfig
    {
        public string countryApiUrl { get; set; }
        public string ApiKey { get; set; }
    }

}