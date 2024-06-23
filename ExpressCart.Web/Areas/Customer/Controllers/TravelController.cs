using System.Drawing.Text;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using ExpressCart.DataAccess.Repository;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;
using ExpressCart.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;

namespace ExpressCartWeb.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class TravelController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAPIRepository _apiRepository;
        private readonly HttpClient client;
        private string GetUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                return claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            return null;
        }
        public TravelController(IAPIRepository apiRepository, IUnitOfWork unitOfWork)
        {
            _apiRepository = apiRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index(int categoryId)
        {
            var category = _unitOfWork.Category.Get(u => u.Id == categoryId);

            List<AirportDetails> airportData = await GetAirportDtlsAsync();

            // Select a random airport
            var random = new Random();
            int index = random.Next(airportData.Count);
            var randomAirport = airportData[index];

            var travelvm = new TravelVM
            {
                CategoryId = categoryId,
                CategoryName = category.Name,
                AirportData = airportData,
                RandomAirportCode = randomAirport.Code,
                RandomAirportName = randomAirport.Name,
                Travel = new Travel()
            };

            return View(travelvm);
        } 
        [HttpPost]
        public async Task<IActionResult> Index(TravelVM travelvm)
        {
            var depLoc = travelvm.Travel.DepartureLocation.ToString();
            var destLoc = travelvm.Travel.DestinationLocation.ToString();
            var depDate = travelvm.Travel.DepartureDate.ToString("yyyy-MM-dd");
            var destDate = travelvm.Travel.DestinationDate.ToString("yyyy-MM-dd");
            int adults = travelvm.Travel.Adults_count;
            int childrens = travelvm.Travel.Childerns_count;
            double maxPrice = travelvm.MaxPrice;
            int maxCount = travelvm.MaxCount;
            var currCode = travelvm.CurrencyCode.ToString();
            var travelClass = travelvm.Class.ToString();
            bool nonStop = travelvm.NonStop;

            List<FlightData> flightDetails = await GetFlightDetailsAsync(depLoc, destLoc, depDate, destDate, adults, childrens, travelClass,nonStop, currCode,maxPrice,maxCount);

            return View(flightDetails);
        }
        private async Task<List<AirportDetails>> GetAirportDtlsAsync()
        {
            return await Task.FromResult(new List<AirportDetails>
            {
                new AirportDetails { Code = "COK", Name = "Cochin International Airport" },
                new AirportDetails { Code = "BKK", Name = "Bangkok International Airport" },
                new AirportDetails { Code = "DXB", Name = "Dubai International Airport" },
                new AirportDetails { Code = "JFK", Name = "John F. Kennedy International Airport" },
                new AirportDetails { Code = "LAX", Name = "Los Angeles International Airport" },
                new AirportDetails { Code = "ORD", Name = "O'Hare International Airport" }
            });
        }

        private async Task<List<FlightData>> GetFlightDetailsAsync(string depLoc, string destLoc, string depDate, string destDate, int adults, int childrens,string travelClass, bool nonStop, string currCode, double maxPrice,int maxCount)
        {
            string baseUrl = _apiRepository.GetFlightApiUrl();
            string accessToken = await GetAccessTokenAsync();

            string formattedUrl = string.Format(baseUrl, depLoc, destLoc, depDate, destDate, adults, childrens, travelClass, nonStop.ToString().ToLower(), currCode, maxPrice,maxCount
     );

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage response = await client.GetAsync(formattedUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var root = JsonConvert.DeserializeObject<Root>(json);

                    return root.Data;
                }
                else
                {
                    string message = response.StatusCode.ToString();
                    TempData["success"] = message;
                }

                return new List<FlightData>();
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            string clientId = _apiRepository.GetFlightApiKey();
            string clientSecret = _apiRepository.GetFlightAPISecret();
            string tokenUrl = "https://test.api.amadeus.com/v1/security/oauth2/token";

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                });

                HttpResponseMessage response = await client.PostAsync(tokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);
                    return tokenResponse.AccessToken;
                }
                else
                {
                    throw new Exception("Unable to retrieve access token.");
                }
            }
        }

        private class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }
        }
    }


    public class Root
    {
        public Meta Meta { get; set; }
        public List<FlightData> Data { get; set; }
    }

    public class Meta
    {
        // Define properties according to the JSON structure
    }

    public class FlightData
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public bool InstantTicketingRequired { get; set; }
        public bool NonHomogeneous { get; set; }
        public bool OneWay { get; set; }
        public bool IsUpsellOffer { get; set; }
        public DateTime LastTicketingDate { get; set; }
        public int NumberOfBookableSeats { get; set; }
        public List<Itinerary> Itineraries { get; set; }
        public Price Price { get; set; }
        public PricingOptions PricingOptions { get; set; }
        public List<string> ValidatingAirlineCodes { get; set; }
        public List<TravelerPricing> TravelerPricings { get; set; }
    }

    public class Itinerary
    {
        public string Duration { get; set; }
        public List<Segment> Segments { get; set; }
    }

    public class Segment
    {
        public Departure Departure { get; set; }
        public Arrival Arrival { get; set; }
        public string CarrierCode { get; set; }
        public string Number { get; set; }
        public Aircraft Aircraft { get; set; }
        public Operating Operating { get; set; }
        public string Duration { get; set; }
        public string Id { get; set; }
        public int NumberOfStops { get; set; }
        public bool BlacklistedInEU { get; set; }
    }

    public class Departure
    {
        public string IataCode { get; set; }
        public string Terminal { get; set; }
        public DateTime At { get; set; }
    }

    public class Arrival
    {
        public string IataCode { get; set; }
        public DateTime At { get; set; }
    }

    public class Aircraft
    {
        public string Code { get; set; }
    }

    public class Operating
    {
        public string CarrierCode { get; set; }
    }

    public class Price
    {
        public string Currency { get; set; }
        public string Total { get; set; }
        public string Base { get; set; }
        public List<Fee> Fees { get; set; }
        public string GrandTotal { get; set; }
    }

    public class Fee
    {
        public string Amount { get; set; }
        public string Type { get; set; }
    }

    public class PricingOptions
    {
        public List<string> FareType { get; set; }
        public bool IncludedCheckedBagsOnly { get; set; }
    }

    public class TravelerPricing
    {
        public string TravelerId { get; set; }
        public string FareOption { get; set; }
        public string TravelerType { get; set; }
        public Price Price { get; set; }
        public List<FareDetailsBySegment> FareDetailsBySegment { get; set; }
    }

    public class FareDetailsBySegment
    {
        public string SegmentId { get; set; }
        public string Cabin { get; set; }
        public string FareBasis { get; set; }
        public string Class { get; set; }
        public IncludedCheckedBags IncludedCheckedBags { get; set; }
    }

    public class IncludedCheckedBags
    {
        public int Weight { get; set; }
        public string WeightUnit { get; set; }
    }


}