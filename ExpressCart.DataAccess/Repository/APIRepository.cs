using ExpressCart.DataAccess.Data;
using ExpressCart.DataAccess.Repository;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;
using ExpressCart.Utility;
using Microsoft.Extensions.Options;

namespace ExpressCart.DataAccess.Repository
{
    public class APIRepository : IAPIRepository
    {
        private readonly API _apiConfig;

        public APIRepository(IOptions<API> apiConfig)
        {
            _apiConfig = apiConfig.Value;
        }

        public string GetRazorSecretKey()
        {
            return _apiConfig.RazorAPI.SecretKey;
        }

        public string GetRazorPublishableKey()
        {
            return _apiConfig.RazorAPI.PublishableKey;
        }

        public string GetFlightApiUrl()
        {
            return _apiConfig.FlightAPI.Url;
        }

        public string GetFlightApiKey()
        {
            return _apiConfig.FlightAPI.ApiKey;
        }

        public string GetFlightAPISecret()
        {
            return _apiConfig.FlightAPI.APISecret;
        }

        public string GetCityApiUrl()
        {
            return _apiConfig.FlightAPI.CityUrl;
        }
    }
}
