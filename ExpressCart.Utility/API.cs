using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCart.Utility
{
    public class API
    {
        //RazorAPI
        public string SecretKey { get; set; }
        public string PublishableKey { get; set; }
        //CountryAPI
        public string countryApiUrl { get; set; }
        public string ApiKey { get; set; }
    }
}